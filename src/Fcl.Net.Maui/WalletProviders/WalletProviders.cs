using Fcl.Net.Core;

namespace Fcl.Net.Maui.WalletProviders
{
    public static class WalletProviders
    {
        public static IEnumerable<FclWalletProvider> Providers(List<FclWalletProvider> customFclWalletProviders = null, ChainId chainId = ChainId.Mainnet)
        {
            var providers = new List<FclWalletProvider>
            {
                new FclWalletProvider
                {
                    Name = "Blocto",
                    Logo = new Uri("https://raw.githubusercontent.com/Outblock/fcl-swift/main/Assets/blocto/logo.jpg"),
                    Method = FclServiceMethod.HttpPost,
                    Endpoint = chainId == ChainId.Mainnet ? new Uri("https://flow-wallet.blocto.app/api/flow/authn").AbsoluteUri : new Uri("https://flow-wallet-testnet.blocto.app/api/flow/authn").AbsoluteUri
                }
            };

            if (customFclWalletProviders != null)
                providers.AddRange(customFclWalletProviders);

            return providers;
        }
    }
}
