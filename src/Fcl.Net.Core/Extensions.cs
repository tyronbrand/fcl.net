using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Core.Cadence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fcl.Net.Core
{
    public static class Extensions
    {
        public static Dictionary<K, V> ToDictionary<K, V>(this object data)
        {
            return JsonConvert.DeserializeObject<Dictionary<K, V>>(JsonConvert.SerializeObject(data));
        }

        public static ICollection<FclArgument> ToFclArgument(this IList<ICadence> arguments)
        {
            var fclArgs = new List<FclArgument>();
            foreach (var arg in arguments)
            {
                var cadenceDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(arg.Encode());

                cadenceDict.TryGetValue("value", out var valueDict);

                if (string.IsNullOrEmpty(valueDict))
                    valueDict = string.Empty;

                fclArgs.Add(
                    new FclArgument
                    {
                        Kind = "ARGUMENT",
                        AsArgument = new FclAsArgument
                        {
                            Type = arg.Type,
                            Value = valueDict
                        },
                        Value = valueDict,
                        Xform = new FclXform
                        {
                            Label = arg.Type
                        },
                        TempId = Guid.NewGuid().ToString("n").Substring(0, 10).ToLower()
                    });
            }

            return fclArgs;
        }
    }
}
