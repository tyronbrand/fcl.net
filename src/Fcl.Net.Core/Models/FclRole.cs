using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclRole
    {
        [JsonProperty("authorizer")]
        public bool Authorizer { get; set; }

        [JsonProperty("payer")]
        public bool Payer { get; set; }

        [JsonProperty("proposer")]
        public bool Proposer { get; set; }

        [JsonProperty("param")]
        public bool Param { get; set; }

        public void Merge(FclRole fclRole)
        {
            Payer = Payer || fclRole.Payer;
            Authorizer = Authorizer || fclRole.Authorizer;
            Proposer = Proposer || fclRole.Proposer;
        }
    }
}
