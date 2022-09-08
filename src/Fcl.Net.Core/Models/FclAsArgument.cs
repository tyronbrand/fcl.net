using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclAsArgument
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
