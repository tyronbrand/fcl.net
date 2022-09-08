using Newtonsoft.Json;
using System;

namespace Fcl.Net.Core.Models
{
    public class FclAppInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("icon")]
        public Uri Icon { get; set; }
    }
}
