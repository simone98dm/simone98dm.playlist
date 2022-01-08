using simone98dm.playlist.lib;

List<string> paths = args.ToList();
if (paths.Count <= 0)
{
    Console.WriteLine("Give me the folder path:");
    string? path = Console.ReadLine();
    if (path == null)
    {
        return;
    }
    paths.Add(path);
}

foreach (string path in paths)
{
    Log.Info($"Reading '{path}'");
    ValidatePath(path);

    string _userToken = "";
    string _userId = "";
    ValidateUserCredentials(_userToken, _userId);

    SporkPlaylistUtils playlistUtils = new SporkPlaylistUtils(_userId, _userToken);

    List<string> songsUris = await playlistUtils.GetFileList(path);

    string title = $"Playlist-#{new Random().Next(1, 100)}";
    string? playlistid = await playlistUtils.CreateNewPlaylist(title);

    await playlistUtils.AddSongsToPlaylist(playlistid, songsUris);
}

void ValidatePath(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        throw new ArgumentNullException(nameof(path));
    }

    if (!Directory.Exists(path))
    {
        throw new Exception("Path not found!");
    }
}

void ValidateUserCredentials(string? _userToken, string? _userId)
{
    if (string.IsNullOrWhiteSpace(_userToken))
    {
        throw new ArgumentNullException(nameof(_userToken));
    }

    if (string.IsNullOrWhiteSpace(_userId))
    {
        throw new ArgumentNullException(nameof(_userId));
    }
}