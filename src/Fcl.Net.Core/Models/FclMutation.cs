using Flow.Net.Sdk.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fcl.Net.Core.Models
{
    public class FclMutation : FlowInteractionBase
    {
        public FclMutation(Dictionary<string, string> addressMap = null)
            : base(addressMap)
        {
            GasLimit = 9999;
        }

        public ulong GasLimit { get; set; }


        public FclInteraction Resolve()
        {
            var arguments = Arguments.ToFclArgument();

            var interaction = new FclInteraction
            {
                Arguments = arguments.ToDictionary(k => k.TempId, v => v),
                Message = new FclMessage
                {
                    Arguments = arguments.Select(s => s.TempId).ToList(),
                    Cadence = Script,
                    ComputeLimit = GasLimit,
                }
            };



            return interaction;
        }
    }
}
