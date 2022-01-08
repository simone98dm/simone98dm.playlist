using simone98dm.playlist.lib.Models;

namespace simone98dm.playlist.lib
{
    public class SporkPlaylistUtils
    {
        private readonly SporkHttpUtils _playlist;

        public SporkPlaylistUtils(string userId, string userToken)
        {
            if (_playlist == null)
            {
                _playlist = new SporkHttpUtils(userId, userToken);
            }
        }

        public async Task<string?> CreateNewPlaylist(string title)
        {
            string? playlistId = null;
            try
            {
                Log.Info($"Creating playlist '{title}'");
                PlaylistOptions playlistOpt = new()
                {
                    description = "Songs from your local folder! by simone98dm",
                    name = title,
                    _public = false
                };
                CreatePlaylistResponse? createPlaylistObj = await _playlist.CreatePlaylistAsync(playlistOpt);
                if (createPlaylistObj != null)
                {
                    playlistId = createPlaylistObj.id;
                    if (string.IsNullOrWhiteSpace(playlistId))
                    {
                        throw new ArgumentNullException(nameof(playlistId));
                    }

                    Log.Success($"Playlist successfully created!");
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
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

                Log.Info($"Retrieve songs info ({files.Count()} songs to search)");
                List<string> titles = files
                    .Select(file => Path.GetFileName(file))
                    .Select(file => file.Replace(Path.GetExtension(file), string.Empty))
                    .ToList();

                ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3 };
                await Parallel.ForEachAsync(titles, parallelOptions, async (title, token) =>
                {
                    TrackInfoResponse? trackInfo = await _playlist.GetTrackInfoAsync(title);

                    if (trackInfo != null)
                    {
                        Item? song = trackInfo.tracks.items.FirstOrDefault();
                        if (song != null)
                        {
                            songs.Add(song.uri);
                        }
                        else
                        {
                            Log.Warning($"Songs '{title}' not found on Spotify");
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            return songs;
        }

        public async Task AddSongsToPlaylist(string? playlistId, List<string> songs)
        {
            if (string.IsNullOrWhiteSpace(playlistId))
            {
                throw new ArgumentNullException(nameof(playlistId));
            }

            if (!songs.Any())
            {
                return;
            }

            try
            {
                Log.Info("Adding songs to playlist...");
                AddSongsPlaylistReponse? response = await _playlist.AddSongsAsync(playlistId, songs.ToArray());
                Log.Success("Songs added successfully!");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

    }
}
