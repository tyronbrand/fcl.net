using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Flow.Net.Sdk.Core.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Resolve
{
    public class FclInteractionBuilder
    {
        private readonly FclServiceConfig _fclServiceConfig;
        private readonly IFlowClient _sdkClient;
        private readonly ExecService _execService;

        public FclInteractionBuilder(ExecService execService, FclServiceConfig fclServiceConfig, IFlowClient sdkClient)
        {
            _fclServiceConfig = fclServiceConfig;
            _sdkClient = sdkClient;
            _execService = execService;
        }

        public async Task<FclInteraction> Build(FclMutation fclMutation, FclUser fclUser)
        {
            var interaction = new FclInteraction
            {
                Tag = FclInteractionTag.Transaction,
                Message = new FclMessage
                {
                    Cadence = fclMutation.Script,
                    ComputeLimit = fclMutation.GasLimit,
                }
            };
            
            if (fclMutation.Arguments.Any())
            {
                var arguments = fclMutation.Arguments.ToFclArgument();
                interaction.Arguments = arguments.ToDictionary(k => k.TempId, v => v);
                interaction.Message.Arguments = arguments.Select(s => s.TempId).ToList();
            }

            var mutationResolvers = new List<IResolver>
            {
                new AccountsResolver(fclUser, _execService, _fclServiceConfig),
                new RefBlockResolver(_sdkClient),
                new SequenceNumberResolver(_sdkClient),
                new SignatureResolver(_execService, _fclServiceConfig)
            };

            foreach (var resolver in mutationResolvers)
                await resolver.Resolve(interaction).ConfigureAwait(false);
            
            return interaction;
        }
    }
}
