# Spork Playlist
Create Spotify playlist by local music folder

To use it, you need to have a Spotify account.

All right and data are linked to Spotify, all rights are reserved to Spotify.

## How it works
Pass the folder path as param. You can pass multiple paths to the console by separating them using the" " (space) character.
The console will search all music by the file name using Spotify API, if the search fails you will see the not found file as console output. When all the folder is scanned, the script will create the playlist on your Spotify account.

### Fun facts
If too many requests are sent to Spotify during 30 seconds, the API will block all requests for 30 seconds. So the console uses a retry policy if the connection return `429 - Too many requests` and retries 3 times before giving up. Also, it uses `Parallel.ForEachAsync()` to create multiple search requests and speed up the execution.

### To remember:
1. Need to get `userid` and `bearer token` from Spotify API, to use the app you should add values to those variables.
    ```
    string _userToken = "";
    string _userId = "";
    ```
2. File name should be right formatted. The script uses the file name to search on Spotify archive the name mustn't start with "01." or "01 - " and whatever. Also sometimes the search will fail if the name of the artist is before the track name and vice-versa (the search fail will display on the console)
