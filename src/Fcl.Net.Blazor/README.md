<br />
<p align="center">
  <h1 align="center">FCL Blazor</h1>
  <p align="center">
    <i>Connect your dapp to users, their wallets and Flow.</i>
    <br />
    <br />
    <a href="https://github.com/tyronbrand/fcl.net/tree/main/examples/BlazorWasm/WasmExample">Blazor Example</a>
  </p>
</p>

## What is FCL?

The Flow Client Library (FCL) is a package used to interact with user wallets and the Flow blockchain. When using FCL for authentication, dapps are able to support all FCL-compatible wallets on Flow and their users without any custom integrations or changes needed to the dapp code.

It was created to make developing applications that connect to the Flow blockchain easy and secure. It defines a standardized set of communication patterns between wallets, applications, and users that is used to perform a wide variety of actions for your dapp. FCL also offers a full featured SDK and utilities to interact with the Flow blockchain.

---
## Getting Started

### Requirements
-  .NET6

### Installing

### [Package Manager Console](#tab/install-with-pmconsole)
Run following command in VS Package Manager Console:  
```
Install-Package Fcl.Net.Blazor
```

### [Command Line](#tab/install-with-cli)
Run following command in command line:  
```
dotnet add package Fcl.Net.Blazor
```
***
---

### Project Setup

#### index.html
Paste the following into the index.html file (<a href="https://github.com/tyronbrand/fcl.net/blob/a7bd8abf1ce6b21594686f457e7a9e3e5fd41c16/examples/BlazorWasm/WasmExample/wwwroot/index.html#L11">wwwroot/index.html</a>)
```html
<script type="text/javascript">
    window.fcl_extensions = []
</script>
```

#### Program.cs
Full example can be seen <a href="https://github.com/tyronbrand/fcl.net/blob/a7bd8abf1ce6b21594686f457e7a9e3e5fd41c16/examples/BlazorWasm/WasmExample/Program.cs#L29">here</a>

Configure SDK server url.
```csharp
var sdkOptions = new FlowClientOptions
{
    ServerUrl = ServerUrl.TestnetHost
};
```

Configure FclConfig
```csharp
var walletDiscoveryConfig = new FclWalletDiscovery
{
    Wallet = new Uri("https://fcl-discovery.onflow.org/testnet/authn"),
    WalletMethod = FclServiceMethod.IFrameRPC
};

var appInfo = new FclAppInfo
{
    Icon = new Uri($"{builder.HostEnvironment.BaseAddress}flow.png"),
    Title = "Blazor Example"
};

var location = builder.HostEnvironment.BaseAddress.Remove(builder.HostEnvironment.BaseAddress.Length - 1);
var fclConfig = new FclConfig(walletDiscoveryConfig, appInfo, location, ChainId.Testnet);
```

Pass configuration to AddFclServices
```csharp
builder.Services.AddFclServices(
    sdkOptions, 
    fclConfig
);
```
