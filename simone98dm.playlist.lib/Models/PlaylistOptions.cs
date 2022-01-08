using Newtonsoft.Json;

namespace simone98dm.playlist.lib.Models
{

    public class PlaylistOptions
    {
        public string name { get; set; }
        public string description { get; set; }
        [JsonProperty("public")]
        public bool _public { get; set; }
    }

}
