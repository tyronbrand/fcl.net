using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclSignableMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
