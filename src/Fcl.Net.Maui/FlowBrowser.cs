namespace Fcl.Net.Maui.FlowAuthenticator
{
    public interface IFlowAuthenticator
    {
        Task AuthenticateAsync(Uri url, Uri callbackUrl);
        Task CloseBrowserAsync();
    }

    public static class WebAuthenticator
    {
        public static Task AuthenticateAsync(Uri url, Uri callbackUrl) => Current.AuthenticateAsync(url, callbackUrl);

        static IFlowAuthenticator Current => FlowAuthenticator.WebAuthenticator.Default;

        static IFlowAuthenticator? defaultImplementation;

        public static IFlowAuthenticator Default =>
            defaultImplementation ??= new FlowWebAuthenticatorImplementation();

        internal static void SetDefault(IFlowAuthenticator? implementation) =>
            defaultImplementation = implementation;
    }
}
