using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclAccountProofData
    {
        public FclAccountProofData(string appId, string nonce)
        {
            AppId = appId;
            Nonce = nonce;
        }

        [JsonProperty("accountProofIdentifier")]
        public string AppId { get; set; }

        [JsonProperty("accountProofNonce")]
        public string Nonce { get; set; }
    }

    public class FclAccountProofSignatureData
    {
        public FclAccountProofSignatureData(string address, string nonce, ICollection<FclCompositeSignature> signatures)
        {
            Address = address;
            Nonce = nonce;
            Signatures = signatures;
        }

        public string Address { get; set; }
        public string Nonce { get; set; }
        public ICollection<FclCompositeSignature> Signatures { get; set; }
    }
}
