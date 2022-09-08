using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclProposalKey
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("keyId")]
        public uint? KeyId { get; set; }

        [JsonProperty("sequenceNum")]
        public ulong? SequenceNum { get; set; }
    }
}
