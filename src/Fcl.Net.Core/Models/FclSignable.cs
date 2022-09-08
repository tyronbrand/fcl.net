using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fcl.Net.Core.Models
{
    public class FclSignable : FclTypeVersion
    {
        public FclSignable()
        {
            Args = new List<FclAsArgument>();
            Data = new Dictionary<string, string>();
        }

        public override string FclType { get; set; } = "Signable";
        public override string FclVsn { get; set; } = "1.0.1";

        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("keyId")]
        public uint? KeyId { get; set; }

        [JsonProperty("addr")]
        public string Address { get; set; }

        [JsonProperty("roles")]
        public FclRole Roles { get; set; }

        [JsonProperty("cadence")]
        public string Cadence { get; set; }

        [JsonProperty("args")]
        public ICollection<FclAsArgument> Args { get; set; }

        [JsonProperty("interaction")]
        public FclInteraction Interaction { get; set; }

        [JsonProperty("voucher")]
        public FclVoucher Voucher { get; set; }
    }
}
