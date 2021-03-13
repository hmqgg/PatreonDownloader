using System;
using System.Linq;
using NLog;

namespace PatreonDownloader.Engine.Helpers
{
    internal static class UrlChecker
    {
        /// <summary>
        ///     Checks that url is a valid url and is not blacklisted
        /// </summary>
        /// <param name="url">Url to check</param>
        /// <param name="useBlackList">Will validation fail if url contains blacklisted string?</param>
        /// <returns></returns>
        public static bool IsValidUrl(string url, bool useBlackList = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            Uri uriResult;
            var validationResult = Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
                                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            var blackList = (ConfigurationManager.Configuration["UrlBlackList"] ?? "").Split("|");
            var lowerUrl = url.ToLower();
            var blackListResult = useBlackList && blackList.Any(x => lowerUrl.Contains(x));

            if (blackListResult)
            {
                LogManager.GetCurrentClassLogger().Debug($"{url} is blacklisted");
            }

            return validationResult && !blackListResult;
        }
    }
}
