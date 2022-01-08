# Spork Playlist
Create Spotify playlist by local music folder

To use it, you need to have a Spotify account.

All right and data are linked to Spotify, all rights are reserved to Spotify.

## How it works
Pass folder path as console params. You can pass multiple paths separating using the " " (space) character.
Then the console will start the search using Spotify API, and then when all folder is scanned a playlist will be created.

### Fun facts
This console uses a retry policy if the connection return `429 - Too many requests`, retries 3 times before giving up and uses `Parallel.ForEachAsync()` to create multiple search request

### To remember:
1. Need to get `userid` and `bearer token` from Spotify API, to use the app you should add values to those variables.
    ```
    string _userToken = "";
    string _userId = "";
    ```
2. File name should be right formatted because the script uses the file name to search on Spotify archive the name mustn't start with "01." or "01 - " and whatever. Also sometimes the search will fail if the name of the artist is before the track name and vice-versa (the search fail will display on the console)
