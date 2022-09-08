using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclPreSignable : FclTypeVersion
    {
        public FclPreSignable()
        {
            Args = new List<FclAsArgument>();
            Data = new Dictionary<string, string>();
        }

        public override string FclType { get; set; } = "PreSignable";
        public override string FclVsn { get; set; } = "1.0.1";

        [JsonProperty("roles")]
        public FclRole Roles { get; set; }

        [JsonProperty("cadence")]
        public string Cadence { get; set; }

        [JsonProperty("args")]
        public ICollection<FclAsArgument> Args { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }

        [JsonProperty("interaction")]
        public FclInteraction Interaction { get; set; }

        [JsonProperty("voucher")]
        public FclVoucher Voucher { get; set; }
    }
}
