using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclServiceProvider : FclTypeVersion
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
