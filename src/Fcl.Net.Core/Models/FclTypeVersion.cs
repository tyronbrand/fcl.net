using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public abstract class FclTypeVersion
    {
        [JsonProperty("f_type")]
        public virtual string FclType { get; set; }

        [JsonProperty("f_vsn")]
        public virtual string FclVsn { get; set; }
    }
}
