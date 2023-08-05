using Fcl.Net.Blazor;
using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using WasmExample;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// FCL //

var sdkOptions = new FlowClientOptions
{
    ServerUrl = ServerUrl.TestnetHost
};

var walletDiscoveryConfig = new FclWalletDiscovery
{
    Wallet = new Uri("https://fcl-discovery.onflow.org/testnet/authn"),
    WalletMethod = FclServiceMethod.IFrameRpc
};

var appInfo = new FclAppInfo
{
    Icon = new Uri($"{builder.HostEnvironment.BaseAddress}flow.png"),
    Title = "Blazor Example"
};

var location = builder.HostEnvironment.BaseAddress.Remove(builder.HostEnvironment.BaseAddress.Length - 1);

var fclConfig = new FclConfig(walletDiscoveryConfig, appInfo, location, ChainId.Testnet)
{
    AccountProof = new FclAccountProofData("AWESOME-BLAZOR-APP-ID", "3037366134636339643564623330316636626239323161663465346131393662")
};

builder.Services.AddFclServices(
    sdkOptions,
    fclConfig
);

await builder.Build().RunAsync();
