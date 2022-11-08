using System;
using System.Collections;
using System.Collections.Generic;

namespace Fcl.Net.Core.Models
{
    public class FclWalletDiscovery
    {
        public FclWalletDiscovery()
        {
            Include = new List<string>();
        }
        // Testnet "https://fcl-discovery.onflow.org/testnet/authn"
        // Mainnet "https://fcl-discovery.onflow.org/authn"
        public Uri Wallet { get; set; }
        public FclServiceMethod WalletMethod { get; set; }
        public ICollection<string> Include { get; set; }
    }
}
