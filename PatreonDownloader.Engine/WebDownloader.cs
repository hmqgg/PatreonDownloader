using NLog;
using PatreonDownloader.Common.Models;
using PatreonDownloader.Engine.Exceptions;
using PatreonDownloader.PuppeteerEngine;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace PatreonDownloader.Engine
{
    //TODO: Make disposable?
    internal sealed class WebDownloader : IWebDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IPuppeteerEngine _puppeteerEngine;

        public WebDownloader(DIParameters diParameters, IPuppeteerEngine puppeteerEngine)
        {
            _puppeteerEngine = puppeteerEngine ?? throw new ArgumentNullException(nameof(puppeteerEngine));

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = diParameters.CookieContainer ?? throw new ArgumentNullException(nameof(diParameters.CookieContainer))
            };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");
        }

        /// <summary>
        ///     Download file
        /// </summary>
        /// <param name="url">File url</param>
        /// <param name="path">Path where the file should be saved</param>
        /// <param name="overwrite"></param>
        public async Task DownloadFile(string url, string path, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(url));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(path));
            }

            if (File.Exists(path))
            {
                if (new FileInfo(path).Length == 0)
                {
                    // Must overwrite if empty.
                    _logger.Info($"File {path} already exists but is empty, will be overwritten!");
                }
                else if (!overwrite)
                {
                    throw new DownloadException($"File {path} already exists");
                }

                _logger.Warn($"File {path} already exists, will be overwriten!");
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                await using Stream contentStream = await (await _httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                _logger.Debug($"Starting download: {url}");
                await contentStream.CopyToAsync(stream);
                _logger.Debug($"Finished download: {url}");
            }
            catch (Exception ex)
            {
                throw new DownloadException($"Unable to download from {url}: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Download url as string data
        /// </summary>
        /// <param name="url">Url to download</param>
        /// <returns>String</returns>
        public async Task<string> DownloadString(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(url));
            }

            try
            {
                var browser = await _puppeteerEngine.GetBrowser();
                var page = await browser.NewPageAsync();
                await page.GoToAsync(url);

                var content = await page.GetContentAsync();
                await page.CloseAsync();

                content = content
                    .Replace("<html><head></head><body><pre style=\"word-wrap: break-word; white-space: pre-wrap;\">", "")
                    .Replace("</pre></body></html>", "");
                return HttpUtility.HtmlDecode(content);
            }
            catch (Exception ex)
            {
                throw new DownloadException($"Unable to download from {url}: {ex.Message}", ex);
            }
        }
    }
}
