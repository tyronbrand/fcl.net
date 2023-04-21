using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclDiscoveryService
    {
        public FclDiscoveryService()
        {
            ClientServices = new List<FclService>();
            SupportedStrategies = new List<FclServiceMethod>();
        }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FclServiceType Type { get; set; }

        [JsonProperty("include", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Include { get; set; }

        [JsonProperty("clientServices", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FclService> ClientServices { get; set; }

        [JsonProperty("supportedStrategies", NullValueHandling = NullValueHandling.Ignore)]        
        public ICollection<FclServiceMethod> SupportedStrategies { get; set; }

        [JsonProperty("network")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChainId Network { get; set; }
    }
}
