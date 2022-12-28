using Fcl.Net.Core.Models;
using System.Collections.Generic;

namespace Fcl.Net.Core.Config
{
    public class FclConfig
    {
        public FclConfig(FclAppInfo appInfo, ChainId environment)
        {
            WalletDiscovery = new FclWalletDiscovery();
            AppInfo = appInfo;
            Location = "";
            Environment = environment;
        }

        public FclConfig(FclWalletDiscovery walletDiscovery, FclAppInfo appInfo, string location, ChainId environment)
        {
            WalletDiscovery = walletDiscovery;
            AppInfo = appInfo;
            Location = location;
            Environment = environment;
        }

        public FclWalletDiscovery WalletDiscovery { get; set; }
        public FclAppInfo AppInfo { get; }
        public string Location { get; }
        public Dictionary<string, string> Services { get; set; }
        public FclAccountProofData AccountProof { get; set; }
        public ChainId Environment { get; set; }
    }
}
