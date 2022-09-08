using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclSignableUser
    {
        [JsonProperty("addr")]
        public string Address { get; set; }

        [JsonProperty("keyId")]
        public uint KeyId { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("role")]
        public FclRole Role { get; set; }

        [JsonProperty("sequenceNum", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? SequenceNumber { get; set; }

        [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }

        [JsonProperty("tempId")]
        public string TempId { get; set; }

        [JsonIgnore]
        public FclService SigningService { get; set; }
    }
}
