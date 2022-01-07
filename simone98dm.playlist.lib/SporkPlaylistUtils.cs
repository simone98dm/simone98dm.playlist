using Newtonsoft.Json;
using simone98dm.playlist.lib.Models;
using System.Net.Http.Headers;
using System.Web;

namespace simone98dm.playlist.lib
{
    public class SporkPlaylistUtils
    {
        private HttpClient _client;
        private readonly string _userToken;
        private readonly string _userId;

        public SporkPlaylistUtils(string userId, string userToken)
        {
            _userId = userId;
            _userToken = userToken;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
        }

        public async Task<string?> CreateNewPlaylist(string title)
        {
            Info($"Creating playlist '{title}'");
            PlaylistOptions playlistOpt = new PlaylistOptions()
            {
                description = "Songs from your local folder! by simone98dm",
                name = title,
                _public = false
            };

            string content = JsonConvert.SerializeObject(playlistOpt);
            StringContent? body = new StringContent(content);
            HttpResponseMessage? createPlaylistResponse = await _client.PostAsync($"https://api.spotify.com/v1/users/{_userId}/playlists", body);
            if (!createPlaylistResponse.IsSuccessStatusCode)
            {
                throw new Exception(await createPlaylistResponse.Content.ReadAsStringAsync());
            }
            SpotifyCreatePlaylistResponse? createPlaylistObj = JsonConvert.DeserializeObject<SpotifyCreatePlaylistResponse>(await createPlaylistResponse.Content.ReadAsStringAsync());
            if (createPlaylistObj == null)
            {
                throw new ArgumentNullException(nameof(createPlaylistObj));
            }
            string? playlistid = createPlaylistObj.id;

            if (string.IsNullOrWhiteSpace(playlistid))
            {
                throw new ArgumentException(nameof(playlistid));
            }

            Success("Playlist successfully created!");
            return playlistid;
        }

        public async Task<List<string>> GetFileList(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            IEnumerable<string>? files = Directory.EnumerateFiles(path, "*.mp3", SearchOption.AllDirectories);

            Info($"Retrieve songs info ({files.Count()} items)");

            List<string> songs = new List<string>();
            foreach (string? file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    continue;
                }

                string? fileName = Path.GetFileName(file);
                fileName = fileName.Replace(Path.GetExtension(file), string.Empty);

                System.Collections.Specialized.NameValueCollection? queryParam = HttpUtility.ParseQueryString(string.Empty);
                queryParam.Add("q", HttpUtility.UrlEncode(fileName));
                queryParam.Add("type", "track");
                queryParam.Add("limit", "1");
                string apiUrl = $"https://api.spotify.com/v1/search?{queryParam}";

                HttpResponseMessage? getSongInfo = await _client.GetAsync(apiUrl);
                if (!getSongInfo.IsSuccessStatusCode)
                {
                    throw new Exception(await getSongInfo.Content.ReadAsStringAsync());
                }

                SpotifySongInfoResponse? obj = JsonConvert.DeserializeObject<SpotifySongInfoResponse>(await getSongInfo.Content.ReadAsStringAsync());
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(obj));
                }

                Item? song = obj.tracks.items.FirstOrDefault();
                if (song == null)
                {
                    Warning($"Songs '{fileName}' not found on Spotify");
                    continue;
                }

                songs.Add(song.uri);
            }
            return songs;
        }

        public async Task AddSongsToPlaylist(string? playlistId, List<string> songs)
        {
            Info("Adding songs to playlist...");
            string? uris = string.Join(",", songs);
            System.Collections.Specialized.NameValueCollection? queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams.Add("uris", uris);
            HttpResponseMessage? addItemtoPlaylistResponse = await _client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks?{queryParams}", null);
            if (!addItemtoPlaylistResponse.IsSuccessStatusCode)
            {
                throw new Exception(await addItemtoPlaylistResponse.Content.ReadAsStringAsync());
            }
            Success("Songs added successfully!");
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
