using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public abstract class FclTypeVersion
    {
        [JsonProperty("f_type", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string FclType { get; set; }

        [JsonProperty("f_vsn", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string FclVsn { get; set; }
    }
}
