using Newtonsoft.Json;
using simone98dm.playlist.lib.Models;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Web;

namespace simone98dm.playlist.lib
{
    public class SporkHttpUtils
    {
        private readonly HttpClient _client;
        private readonly string _userToken;
        private readonly string _userId;
        private int _retryCount = 0;

        public SporkHttpUtils(string userId, string userToken)
        {
            _userId = userId;
            _userToken = userToken;

            if (_client == null)
            {
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
            }
        }

        public async Task<TrackInfoResponse?> GetTrackInfoAsync(string title, string type = "track", string limit = "1")
        {
            NameValueCollection? queryParam = HttpUtility.ParseQueryString(string.Empty);
            queryParam.Add("q", HttpUtility.UrlEncode(title));
            queryParam.Add("type", "track");
            queryParam.Add("limit", "1");

            TrackInfoResponse? trackInfo = await RetryAfter<TrackInfoResponse?>(
                async () => await _client.GetAsync($"https://api.spotify.com/v1/search?{queryParam}"),
                 (httpResponse, _) => httpResponse.IsSuccessStatusCode);

            return trackInfo;
        }

        public async Task<CreatePlaylistResponse?> CreatePlaylistAsync(PlaylistOptions options)
        {
            string content = JsonConvert.SerializeObject(options);

            CreatePlaylistResponse? createPlaylistObj = await RetryAfter<CreatePlaylistResponse>(
                async () => await _client.PostAsync($"https://api.spotify.com/v1/users/{_userId}/playlists", new StringContent(content)),
                 (httpResponse, _) => httpResponse.IsSuccessStatusCode);

            return createPlaylistObj;
        }

        public async Task<AddSongsPlaylistReponse?> AddSongsAsync(string playlistId, params string[] songsUri)
        {
            string? uris = string.Join(",", songsUri);
            NameValueCollection? queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams.Add("uris", uris);

            AddSongsPlaylistReponse? addSongs = await RetryAfter<AddSongsPlaylistReponse>(
                 async () => await _client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks?{queryParams}", null),
                 (httpResponse, _) => httpResponse.IsSuccessStatusCode);

            return addSongs;
        }

        private async Task<T?> RetryAfter<T>(Func<Task<HttpResponseMessage>> func, Func<HttpResponseMessage, T, bool> validCondition, int ms = 30000)
        {
            if (_retryCount >= 3)
            {
                _retryCount = 0;
                return default;
            }

            HttpResponseMessage? response = await func.Invoke();
            _retryCount++;
            if (response.IsSuccessStatusCode)
            {
                T? parsedContent = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                if (parsedContent == null)
                {
                    _retryCount = 0;
                    return default;
                }

                bool isValid = validCondition(response, parsedContent);
                if (isValid)
                {
                    _retryCount = 0;
                    return parsedContent;
                }
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Log.Warning("Max limit reached, waiting 30s...");
                Thread.Sleep(ms);
                return await RetryAfter<T>(func, validCondition);

            }

            _retryCount = 0;
            return default;
        }
    }
}
