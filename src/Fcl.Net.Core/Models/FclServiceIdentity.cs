using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclServiceIdentity : FclTypeVersion
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("keyId")]
        public uint KeyId { get; set; }
    }
}
