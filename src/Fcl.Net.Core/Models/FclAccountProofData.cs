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

        public string AppId { get; set; }
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
