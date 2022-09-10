using Flow.Net.Sdk.Core.Cadence.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public class ArrayConverter<T> : JsonConverter<T> where T : new()
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var target = new T();

            if (reader.TokenType == JsonToken.StartArray)
            {
                var jArray = JArray.Load(reader);
                var jObject = jArray.First();
                serializer.Populate(jObject.CreateReader(), target);
            }
            else
            {
                var jObject = JObject.Load(reader);
                serializer.Populate(jObject.CreateReader(), target);
            }

            return target;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) => throw new NotImplementedException();        
    }
}

