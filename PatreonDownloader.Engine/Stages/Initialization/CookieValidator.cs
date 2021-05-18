using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using NLog;
using PatreonDownloader.Engine.Exceptions;

namespace PatreonDownloader.Engine.Stages.Initialization
{
    internal sealed class CookieValidator : ICookieValidator
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IWebDownloader _webDownloader;

        public CookieValidator(IWebDownloader webDownloader)
        {
            _webDownloader = webDownloader ?? throw new ArgumentNullException(nameof(webDownloader));
        }

        public async Task ValidateCookies(CookieContainer cookieContainer)
        {
            if (cookieContainer == null)
            {
                throw new ArgumentNullException(nameof(cookieContainer));
            }

            var cookies = cookieContainer.GetCookies(new Uri("https://patreon.com"));

            if (cookies["__cf_bm"] == null)
            {
                throw new CookieValidationException("__cf_bm cookie not found");
            }

            if (cookies["session_id"] == null)
            {
                throw new CookieValidationException("session_id cookie not found");
            }

            if (cookies["patreon_device_id"] == null)
            {
                throw new CookieValidationException("patreon_device_id cookie not found");
            }

            var apiResponse = await _webDownloader.DownloadString("https://www.patreon.com/api/current_user");

            if (apiResponse.ToLower(CultureInfo.InvariantCulture).Contains("\"status\":\"401\""))
            {
                throw new CookieValidationException("current_user api endpoint returned 401 Unauthorized");
            }
        }
    }
}
