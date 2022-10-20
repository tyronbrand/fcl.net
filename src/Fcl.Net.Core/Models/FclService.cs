using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclService : FclTypeVersion
    {
        public FclService()
        {
            Params = new Dictionary<string, string>();
            Data = new Dictionary<string, object>();
            Headers = new Dictionary<string, string>();
            Scoped = new Dictionary<string, object>();
        }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FclServiceType Type { get; set; }

        [JsonProperty("method")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FclServiceMethod Method { get; set; }

        [JsonProperty("endpoint")]
        public Uri Endpoint { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("identity")]
        public FclServiceIdentity Identity { get; set; }

        [JsonProperty("provider")]
        public FclServiceProvider Provider { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, string> Params { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }

        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("scoped")]
        public Dictionary<string, object> Scoped { get; set; }

        [JsonProperty("network", NullValueHandling = NullValueHandling.Ignore)]
        public string Network { get; set; }
    }
}
