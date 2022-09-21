namespace Fcl.Net.Maui.WebBrowser
{
    public static class WebUtils
    {
        internal static bool CanHandleCallback(Uri expectedUrl, Uri callbackUrl)
        {
            if (!callbackUrl.Scheme.Equals(expectedUrl.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(expectedUrl.Host))
            {
                if (!callbackUrl.Host.Equals(expectedUrl.Host, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}
