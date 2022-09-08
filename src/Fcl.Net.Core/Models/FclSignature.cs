using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclSignature
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("keyId")]
        public uint KeyId { get; set; }

        [JsonProperty("sig", NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }
    }
}
