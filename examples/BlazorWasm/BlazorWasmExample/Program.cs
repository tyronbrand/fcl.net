using System;
using System.Net.Http;
using BlazorWasmExample;
using Fcl.Net.Blazor;
using Fcl.Net.Blazor.Platform;
using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var sdkOptions = new FlowClientOptions
{
    ServerUrl = builder.HostEnvironment.IsProduction() ? ServerUrl.MainnetHost: ServerUrl.TestnetHost
};

var fclConfig =
    new FclConfig(
        new FclWalletDiscovery
        {
            // TODO - Read uri from config
            Wallet = new Uri("https://fcl-discovery.onflow.org/testnet/authn"),
            WalletMethod = FclServiceMethod.IFrameRPC
        },
        new FclAppInfo
        {
            Icon = new Uri("https://kitty-items-flow-testnet-prod.herokuapp.com/images/kitty-items-logo.svg"),
            Title = "Blazor Example"
        },
        builder.HostEnvironment.BaseAddress.Remove(builder.HostEnvironment.BaseAddress.Length - 1),
        ChainId.Testnet
    )
    {
        AccountProof = new FclAccountProofData("AWESOME-BLAZOR-APP-ID", "3037366134636339643564623330316636626239323161663465346131393662")
    };

builder.Services.AddFclServices(sdkOptions, fclConfig);

await builder.Build().RunAsync();
