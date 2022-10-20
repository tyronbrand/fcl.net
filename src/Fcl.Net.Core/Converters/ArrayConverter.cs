using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Fcl.Net.Core.Converters
{
    public class ArrayConverter<T> : JsonConverter<T> where T : new()
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var target = new T();

            if (reader.TokenType == JsonToken.Null)
                return target;

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
