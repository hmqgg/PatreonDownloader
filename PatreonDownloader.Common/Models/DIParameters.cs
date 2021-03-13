using System;
using System.Net;

namespace PatreonDownloader.Common.Models
{
    /// <summary>
    ///     Container for parameters/variables used by dependency injected classes
    /// </summary>
    public class DIParameters
    {
        public DIParameters(CookieContainer cookieContainer, bool isHeadless, Uri remoteBrowserAddress)
        {
            CookieContainer = cookieContainer;
            IsHeadless = isHeadless;
            RemoteBrowserAddress = remoteBrowserAddress;
        }

        /// <summary>
        ///     Patreon/Cloudlfare cookies
        /// </summary>
        public CookieContainer CookieContainer { get; }

        /// <summary>
        ///     Should browser be started in headless mode
        /// </summary>
        public bool IsHeadless { get; }

        /// <summary>
        ///     Address of the remote browser
        /// </summary>
        public Uri RemoteBrowserAddress { get; }
    }
}
