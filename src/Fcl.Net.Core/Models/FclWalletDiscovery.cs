using System;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclWalletDiscovery
    {
        public FclWalletDiscovery()
        {
            Include = new List<string>();
        }

        // UI version of discovery
        // Testnet "https://fcl-discovery.onflow.org/testnet/authn"
        // Mainnet "https://fcl-discovery.onflow.org/authn"
        public Uri Wallet { get; set; }
        public FclServiceMethod WalletMethod { get; set; }
        public ICollection<string> Include { get; set; }

        // API version of discovery
        // Testnet "https://fcl-discovery.onflow.org/api/testnet/authn"
        // Mainnet "https://fcl-discovery.onflow.org/api/authn"
        public Uri Authn { get; set; }
    }
}
