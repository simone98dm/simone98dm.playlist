using Newtonsoft.Json;
using simone98dm.playlist.lib.Models;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Web;

namespace simone98dm.playlist.lib
{
    public class SporkPlaylistUtils
    {
        private readonly HttpClient _client;
        private readonly string _userToken;
        private readonly string _userId;

        public SporkPlaylistUtils(string userId, string userToken)
        {
            _userId = userId;
            _userToken = userToken;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        }

        public async Task<string?> CreateNewPlaylist(string title)
        {
            string? playlistId = null;
            try
            {
                Info($"Creating playlist '{title}'");
                PlaylistOptions playlistOpt = new()
                {
                    description = "Songs from your local folder! by simone98dm",
                    name = title,
                    _public = false
                };

                string content = JsonConvert.SerializeObject(playlistOpt);
                HttpResponseMessage? response = await _client.PostAsync($"https://api.spotify.com/v1/users/{_userId}/playlists", new StringContent(content));
                string? responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(responseContent);
                }

                CreatePlaylistResponse? createPlaylistObj = JsonConvert.DeserializeObject<CreatePlaylistResponse>(responseContent);
                if (createPlaylistObj == null)
                {
                    throw new ArgumentNullException(nameof(createPlaylistObj));
                }

                playlistId = createPlaylistObj.id;
                if (string.IsNullOrWhiteSpace(playlistId))
                {
                    throw new ArgumentNullException(nameof(playlistId));
                }

                Success($"Playlist successfully created!");
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }

            return playlistId;
        }

        public async Task<List<string>> GetFileList(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            List<string> songs = new List<string>();
            try
            {
                IEnumerable<string>? files = Directory.EnumerateFiles(path, "*.mp3", SearchOption.AllDirectories);

                Info($"Retrieve songs info ({files.Count()} songs to search)");
                List<string> titles = files
                    .Select(file => Path.GetFileName(file))
                    .Select(file => file.Replace(Path.GetExtension(file), string.Empty))
                    //.Select(file => file.Substring(5, file.Length - 5)) // added for my case which has "01 - " text before the song title
                    .ToList();

                ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3 };
                await Parallel.ForEachAsync(titles, parallelOptions, async (title, token) =>
                {
                    NameValueCollection? queryParam = HttpUtility.ParseQueryString(string.Empty);
                    queryParam.Add("q", HttpUtility.UrlEncode(title));
                    queryParam.Add("type", "track");
                    queryParam.Add("limit", "1");
                    HttpResponseMessage? response = await _client.GetAsync($"https://api.spotify.com/v1/search?{queryParam}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(responseContent);
                    }

                    TrackInfoResponse? trackInfo = JsonConvert.DeserializeObject<TrackInfoResponse>(responseContent);
                    if (trackInfo == null)
                    {
                        throw new ArgumentNullException(nameof(trackInfo));
                    }

                    Item? song = trackInfo.tracks.items.FirstOrDefault();
                    if (song != null)
                    {
                        songs.Add(song.uri);
                    }
                    else
                    {
                        Warning($"Songs '{title}' not found on Spotify");
                    }
                });
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }

            return songs;
        }

        public async Task AddSongsToPlaylist(string? playlistId, List<string> songs)
        {
            if (string.IsNullOrWhiteSpace(playlistId))
            {
                throw new ArgumentNullException(nameof(playlistId));
            }

            if (songs.Any())
            {
                return;
            }

            try
            {
                Info("Adding songs to playlist...");
                string? uris = string.Join(",", songs);
                NameValueCollection? queryParams = HttpUtility.ParseQueryString(string.Empty);
                queryParams.Add("uris", uris);
                HttpResponseMessage? response = await _client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks?{queryParams}", null);
                string contentResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(contentResponse);
                }
                Success("Songs added successfully!");
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
        }

        public void Info(string text)
        {
            Print(ConsoleColor.White, text);
        }

        public void Error(string text)
        {
            Print(ConsoleColor.Red, $"[x] {text}");
        }

        public void Success(string text)
        {
            Print(ConsoleColor.Green, $"[+] {text}");
        }

        public void Warning(string text)
        {
            Print(ConsoleColor.Yellow, $"[!] {text}");
        }

        private void Print(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
