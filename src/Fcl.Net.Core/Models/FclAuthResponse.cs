using Fcl.Net.Core.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclAuthResponse : FclTypeVersion
    {
        public FclAuthResponse()
        {
            UserSignatures = new List<FclCompositeSignature>();
        }

        [JsonConverter(typeof(ArrayConverter<FclAuthData>))]
        public FclAuthData Data { get; set; }

        [JsonConverter(typeof(ArrayConverter<FclService>))]
        public FclService Local { get; set; }
        public ResponseStatus Status { get; set; }        
        public FclService Updates { get; set; }        
        public string Reason { get; set; }
        public FclAuthData CompositeSignature { get; set; }
        public FclService AuthorizationUpdates { get; set; }
        public ICollection<FclCompositeSignature> UserSignatures { get; set; }
    }    
}

