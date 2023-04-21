using Fcl.Net.Blazor.LocalViews;
using Fcl.Net.Blazor.Platform;
using Fcl.Net.Blazor.Strategies;
using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Fcl.Net.Blazor
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddFclServices(this IServiceCollection services, FlowClientOptions sdkClientOptions, FclConfig fclConfig)
        {
            if (string.IsNullOrWhiteSpace(fclConfig.Location))
                throw new FclException("FclConfig: Location required.");

            if (fclConfig.WalletDiscovery == null)
                throw new FclException("FclConfig: WalletDiscovery required.");

            // platform
            services.AddSingleton<IPlatform, BlazorPlatform>();

            // sdk client            
            services.AddSingleton(f => sdkClientOptions);
            services.AddHttpClient<IFlowClient, FlowHttpClient>();

            // fetch service
            services.AddSingleton(f =>
            {
                var fetchServiceConfig = new FetchServiceConfig
                {
                    Location = fclConfig.Location
                };

                return fetchServiceConfig;
            });
            services.AddHttpClient<FetchService>();

            // JS interop
            services.AddSingleton<FclJsObjRef>();

            // local views
            services.AddSingleton<IFrameLocalView>();
            services.AddSingleton<PopLocalView>();

            // strategies
            services.AddSingleton<JsStrategy>();
            services.AddSingleton(f =>
            {
                // local views
                var localViews = new Dictionary<FclServiceMethod, ILocalView>
                {
                    { FclServiceMethod.ViewIFrame, f.GetRequiredService<IFrameLocalView>() },
                    { FclServiceMethod.ViewPop, f.GetRequiredService<PopLocalView>() },
                };

                return new HttpPostStrategy(f.GetRequiredService<FetchService>(), localViews);
            });            

            // fcl
            services.AddSingleton(f =>
            {
                // strategies
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.HttpPost, f.GetRequiredService<HttpPostStrategy>() },
                    { FclServiceMethod.IFrameRPC, f.GetRequiredService<JsStrategy>() },
                    { FclServiceMethod.PopRpc, f.GetRequiredService<JsStrategy>() },
                    { FclServiceMethod.ExtRpc, f.GetRequiredService<JsStrategy>() },
                    { FclServiceMethod.WcRpc, f.GetRequiredService<JsStrategy>() }
                };

                return new Core.Fcl(
                    fclConfig,
                    f.GetRequiredService<IFlowClient>(),
                    f.GetRequiredService<IPlatform>(),
                    strategies);
            });            

            return services;
        }
    }
}
