using Newtonsoft.Json;
using System;

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

        [JsonProperty("website", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Website { get; set; }

        [JsonProperty("supportEmail", NullValueHandling = NullValueHandling.Ignore)]
        public string SupportEmail { get; set; }
    }
}
