using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

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
