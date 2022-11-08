using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public string Endpoint { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("identity", NullValueHandling = NullValueHandling.Ignore)]
        public FclServiceIdentity Identity { get; set; }

        [JsonProperty("provider")]
        public FclServiceProvider Provider { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, string> Params { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Data { get; set; }

        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty("scoped", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Scoped { get; set; }

        [JsonProperty("optIn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OptIn { get; set; }
    }
}
