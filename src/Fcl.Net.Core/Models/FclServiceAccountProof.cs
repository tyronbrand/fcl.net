using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclServiceAccountProof : FclTypeVersion
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("signatures")]
        public ICollection<FclCompositeSignature> Signatures { get; set; }
    }
}
