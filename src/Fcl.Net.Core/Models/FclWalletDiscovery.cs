using System;

namespace Fcl.Net.Core.Models
{
    public class FclWalletDiscovery
    {
        // Testnet "https://fcl-discovery.onflow.org/testnet/authn"
        // Mainnet "https://fcl-discovery.onflow.org/authn"
        public Uri Wallet { get; set; }
        public FclServiceMethod WalletMethod { get; set; }
    }
}
