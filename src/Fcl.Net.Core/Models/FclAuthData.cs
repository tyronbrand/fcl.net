using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclAuthData : FclService
    {
        public FclAuthData()
        {
            Services = new List<FclService>();
            Payer = new List<FclService>();
            Authorization = new List<FclService>();
        }

        [JsonProperty("addr")]
        public string Address { get; set; }

        [JsonProperty("services")]
        public ICollection<FclService> Services { get; set; }

        [JsonProperty("keyId")]
        public uint? KeyId { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("proposer")]
        public FclService Proposer { get; set; }

        [JsonProperty("payer")]
        public ICollection<FclService> Payer { get; set; }

        [JsonProperty("authorization")]
        public ICollection<FclService> Authorization { get; set; }
    }
}
