using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Core.Client;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Resolve
{
    public class RefBlockResolver : IResolver
    {
        private readonly IFlowClient _sdkClient;
        public RefBlockResolver(IFlowClient sdkClient)
        {
            _sdkClient = sdkClient;
        }

        public async Task Resolve(FclInteraction fclInteraction)
        {
            var block = await _sdkClient.GetLatestBlockAsync(false).ConfigureAwait(false);
            fclInteraction.Message.RefrenceBlock = block.Header.Id;
        }
    }
}
