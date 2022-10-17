using Fcl.Net.Core.Config;
using Fcl.Net.Core.Models;
using Fcl.Net.Core;
using Fcl.Net.Maui;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;

namespace MauiExample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
#if TIZEN
				.UseMauiCompatibility()
#endif
                .ConfigureEssentials(essentials =>
                {
                    essentials.UseVersionTracking();
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var sdkOptions = new FlowClientOptions
            {
                ServerUrl = ServerUrl.TestnetHost // TODO - check if prod/dev
            };

            var fclConfig =
                new FclConfig(
                    new FclWalletDiscovery
                    {
                        // TODO - Read uri from config
                        Wallet = new Uri("https://fcl-discovery.onflow.org/testnet/authn"),
                        WalletMethod = FclServiceMethod.HttpPost
                    },
                    new FclAppInfo
                    {
                        Icon = new Uri("https://kitty-items-flow-testnet-prod.herokuapp.com/images/kitty-items-logo.svg"),
                        Title = "Blazor Example"
                    },
                    "",
                    ChainId.Testnet
                )
                {
                    AccountProof = new FclAccountProofData("AWESOME-BLAZOR-APP-ID", "3037366134636339643564623330316636626239323161663465346131393662")
                };

            builder.Services.AddFclServices(sdkOptions, fclConfig, new Uri("fclmaui://"));


            return builder.Build();
        }
    }
}