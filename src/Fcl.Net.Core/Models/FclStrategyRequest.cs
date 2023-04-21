using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclStrategyRequest
    {
        public FclStrategyRequest()
        {
            FclVersion = Core.FclVersion.Number;
        }

        [JsonProperty("fclVersion")]
        public string FclVersion { get; }

        [JsonProperty("config")]
        public FclServiceConfig Config { get; set; }

        [JsonProperty("service")]
        public FclStrategyRequestService Service { get; set; }
    }

    public class FclStrategyRequestService
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FclServiceType Type { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Params { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Data { get; set; }
    }
}
