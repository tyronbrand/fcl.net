using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclUser : FclTypeVersion
    {
        public FclUser()
        {
            Services = new List<FclService>();
        }

        public override string FclType { get; set; } = "User";
        public override string FclVsn { get; set; } = "1.0.0";

        [JsonProperty("addr")]
        public string Address { get; set; }

        [JsonProperty("loggedIn")]
        public bool LoggedIn { get; set; }

        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty("services")]
        public ICollection<FclService> Services { get; set; }
    }
}
