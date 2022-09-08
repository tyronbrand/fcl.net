using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclVoucher
    {
        public FclVoucher()
        {
            Arguments = new List<FclAsArgument>();
            Authorizers = new List<string>();
            PayloadSignatures = new List<FclSignature>();
            EnvelopeSignatures = new List<FclSignature>();
        }

        [JsonProperty("arguments")]
        public ICollection<FclAsArgument> Arguments { get; set; }

        [JsonProperty("authorizers")]
        public ICollection<string> Authorizers { get; set; }

        [JsonProperty("cadence")]
        public string Cadence { get; set; }

        [JsonProperty("computeLimit")]
        public ulong ComputeLimit { get; set; }

        [JsonProperty("payer")]
        public string Payer { get; set; }

        [JsonProperty("payloadSigs")]
        public ICollection<FclSignature> PayloadSignatures { get; set; }

        [JsonProperty("envelopeSigs")]
        public ICollection<FclSignature> EnvelopeSignatures { get; set; }

        [JsonProperty("proposalKey")]
        public FclProposalKey ProposalKey { get; set; }

        [JsonProperty("refBlock")]
        public string RefrenceBlock { get; set; }
    }
}
