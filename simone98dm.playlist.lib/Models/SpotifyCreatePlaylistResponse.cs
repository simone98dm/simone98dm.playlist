using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simone98dm.playlist.lib.Models
{
    public class SpotifyCreatePlaylistResponse
    {
        public bool collaborative { get; set; }
        public string description { get; set; }
        public CreatePlaylistExternal_Urls external_urls { get; set; }
        public Followers followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public object[] images { get; set; }
        public string name { get; set; }
        public CreatePlaylistOwner owner { get; set; }
        public object primary_color { get; set; }
        public bool _public { get; set; }
        public string snapshot_id { get; set; }
        public CreatePlaylistTracks tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class CreatePlaylistExternal_Urls
    {
        public string spotify { get; set; }
    }

    public class Followers
    {
        public object href { get; set; }
        public int total { get; set; }
    }

    public class CreatePlaylistOwner
    {
        public string display_name { get; set; }
        public CreatePlaylistExternal_Urls1 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class CreatePlaylistExternal_Urls1
    {
        public string spotify { get; set; }
    }

    public class CreatePlaylistTracks
    {
        public string href { get; set; }
        public object[] items { get; set; }
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public int total { get; set; }
    }

}
