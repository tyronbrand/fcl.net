using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclServiceConfig
    {
        public FclServiceConfig()
        {
            Services = new Dictionary<string, string>();
        }

        [JsonProperty("services", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Services { get; set; }

        [JsonProperty("app")]
        public FclAppInfo App { get; set; }

        [JsonProperty("client")]
        public FclClientInfo Client { get; set; }
    }
}
