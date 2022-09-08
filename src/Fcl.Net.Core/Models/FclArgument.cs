using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclArgument
    {
        [JsonProperty("asArgument")]
        public FclAsArgument AsArgument { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("tempId")]
        public string TempId { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("xform")]
        public FclXform Xform { get; set; }
    }

    public class FclXform
    {
        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
