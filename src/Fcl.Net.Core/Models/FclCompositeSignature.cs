using Newtonsoft.Json;

namespace Fcl.Net.Core.Models
{
    public class FclCompositeSignature : FclTypeVersion
    {
        public override string FclType { get; set; } = "CompositeSignature";
        public override string FclVsn { get; set; } = "1.0.0";

        [JsonProperty("addr")]
        public string Address { get; set; }

        [JsonProperty("keyId")]
        public uint KeyId { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
