<br />
<p align="center">
  <h1 align="center">FCL MAUI</h1>
  <p align="center">
    <i>Connect your dapp to users, their wallets and Flow.</i>
    <br />
    <br />
    <a href="https://github.com/tyronbrand/fcl.net/tree/main/examples/Maui/MauiExample">MAUI Example</a>
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
Install-Package Fcl.Net.Maui
```

### [Command Line](#tab/install-with-cli)
Run following command in command line:  
```
dotnet add package Fcl.Net.Maui
```
***
---

### Project Setup

#### MauiProgram.cs
Full example can be seen <a href="https://github.com/tyronbrand/fcl.net/blob/f1fcab793ceac7343454cc33b69224039402d962/examples/Maui/MauiExample/MauiProgram.cs#L30">here</a>

Configure SDK server url.
```csharp
var sdkOptions = new FlowClientOptions
{
    ServerUrl = ServerUrl.TestnetHost
};
```

Configure FclConfig
```csharp
// fcl config
var appInfo = new FclAppInfo
{
    Icon = new Uri("https://avatars.githubusercontent.com/u/62387156?s=200&v=4"),
    Title = "Blazor Example"
};

var fclConfig = new FclConfig(appInfo, ChainId.Testnet)
{
    AccountProof = new FclAccountProofData("AWESOME-BLAZOR-APP-ID", "3037366134636339643564623330316636626239323161663465346131393662")
};

// maui redirect URI
var redirectUri = new Uri("enterAppUri://");

builder.Services.AddFclServices(sdkOptions, fclConfig, redirectUri);
```

#### iOS setup
Add the following to Info.plist. Example can be found <a href="https://github.com/tyronbrand/fcl.net/blob/main/examples/Maui/MauiExample/Platforms/iOS/Info.plist">here</a>

```
<array>
		<dict>
			<key>CFBundleURLName</key>
			<string>enterAppUri</string>
			<key>CFBundleURLSchemes</key>
			<array>
				<string>enterAppUri</string>
			</array>
			<key>CFBundleTypeRole</key>
			<string>Editor</string>
		</dict>
	</array>
```

### Andriod setup
Add the following to AndriodManifest.xml. Example can be found <a href="https://github.com/tyronbrand/fcl.net/blob/main/examples/Maui/MauiExample/Platforms/Android/AndroidManifest.xml">here</a>

```
<intent>
    <action android:name="android.intent.action.VIEW" />
    <data android:scheme="https" />
</intent>
<intent>
    <action android:name="android.support.customtabs.action.CustomTabsService" />
</intent>
```

Secondly, create an Activity to handle the redirect. Example can be found <a href="https://github.com/tyronbrand/fcl.net/blob/f1fcab793ceac7343454cc33b69224039402d962/examples/Maui/MauiExample/Platforms/Android/MainActivity.cs#L54">here</a>

```csharp
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },        
    DataScheme = "fclmaui")]
public class FclCallbackActivity : Activity
{
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        var intent = new Intent(this, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        StartActivity(intent);

        Finish();
    }
}
```