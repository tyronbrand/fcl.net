using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclClientInfo
    {
        public FclClientInfo()
        {
            FclVersion = Core.FclVersion.Number;
            FclLibrary = "https://github.com/tyronbrand/fcl.net";
        }

        [JsonProperty("fclVersion")]
        public string FclVersion { get; }

        [JsonProperty("fclLibrary")]
        public string FclLibrary { get; }

        [JsonProperty("hostname", NullValueHandling = NullValueHandling.Ignore)]
        public string Hostname { get; set; }

        [JsonProperty("clientServices", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientServices { get; set; }

        [JsonProperty("supportedStrategies", NullValueHandling = NullValueHandling.Ignore)]
        public string SupportedStrategies { get; set; }
    }
}
