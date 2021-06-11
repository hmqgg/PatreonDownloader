using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using NLog;
using PatreonDownloader.Common.Models;
using PatreonDownloader.PuppeteerEngine.Wrappers.Browser;
using PuppeteerSharp;

namespace PatreonDownloader.PuppeteerEngine
{
    public class PuppeteerEngine : IPuppeteerEngine, IDisposable
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Browser _browser;
        private IWebBrowser _browserWrapper;

        private Uri _remoteBrowserAddress;

        /// <summary>
        ///     Create a new instance of PuppeteerEngine using remote browser
        /// </summary>
        /// <param name="remoteBrowserAddress">Address of the remote browser with remote debugging enabled</param>
        public PuppeteerEngine(Uri remoteBrowserAddress)
        {
            if (remoteBrowserAddress == null)
            {
                throw new ArgumentNullException(nameof(remoteBrowserAddress));
            }

            Initialize(remoteBrowserAddress, true);
        }

        /// <summary>
        ///     Create a new instance of PuppeteerEngine using local browser
        /// </summary>
        /// <param name="headless">If set to false then the browser window will be visible</param>
        public PuppeteerEngine(bool headless = true)
        {
            Initialize(null, headless);
        }

        /// <summary>
        ///     Create a new instance of PuppeteerEngine
        /// </summary>
        /// <param name="diParameters">Dependency injection parameters container</param>
        [Inject]
        public PuppeteerEngine(DIParameters diParameters)
        {
            Initialize(diParameters.RemoteBrowserAddress, diParameters.IsHeadless);
        }

        public bool IsHeadless { get; private set; }

        public async Task<IWebBrowser> GetBrowser()
        {
            if (_browser != null && !_browser.IsClosed)
            {
                return _browserWrapper;
            }

            if (_remoteBrowserAddress == null)
            {
                return await StartLocalBrowser();
            }

            return await ConnectToRemoteBrowser();
        }

        public async Task CloseBrowser()
        {
            if (_remoteBrowserAddress != null)
            {
                return;
            }

            if (_browser != null && !_browser.IsClosed)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
                _browser = null;
            }
        }

        public void Dispose()
        {
            if (_remoteBrowserAddress != null)
            {
                return;
            }

            _logger.Debug("Disposing puppeteer engine");
            _browser?.Dispose();
        }

        private void Initialize(Uri remoteBrowserAddress, bool headless)
        {
            _logger.Debug($"Initializing PuppeteerEngine with parameters {remoteBrowserAddress}, {headless}");

            if (remoteBrowserAddress != null)
            {
                IsHeadless = true;
                _remoteBrowserAddress = remoteBrowserAddress;
            }
            else
            {
                IsHeadless = headless;
                KillChromeIfRunning();
            }
        }

        private void KillChromeIfRunning()
        {
            var processList = Process.GetProcessesByName("chrome");
            if (processList.Length > 0)
            {
                _logger.Debug($"Found {processList.Length} chrome processes (not sure which one yet)");

                processList = processList.Where(x =>
                        x.MainModule != null && x.MainModule.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))
                    .ToArray();
                if (processList.Length > 0)
                {
                    _logger.Debug($"{processList.Length} chrome processes are in patreondownloader's folder");
                    _logger.Warn("Running PatreonDownloader's Chrome detected. Attempting to close it...");

                    var failed = false;
                    foreach (var process in processList)
                    {
                        _logger.Debug($"Attempting to kill PID {process.Id}");
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            failed = true;
                            _logger.Error($"Error while closing chrome: {ex}");
                        }
                    }

                    if (failed)
                    {
                        _logger.Error(
                            "Unable to close some or all PatreonDownloader's Chrome instances. Please close them manually via process manager if you encounter any problems running this application.");
                    }
                    else
                    {
                        _logger.Info("Successfully killed all PatreonDownloader's Chrome instances.");
                    }
                }
            }
        }

        /// <summary>
        ///     Initialize locally running browser
        /// </summary>
        /// <returns></returns>
        private async Task<IWebBrowser> StartLocalBrowser()
        {
            try
            {
                _logger.Debug("Downloading browser");
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                _logger.Debug("Launching browser");
                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    //Devtools = true,
                    Headless = IsHeadless,
                    UserDataDir = Path.Combine(Environment.CurrentDirectory, "chromedata"),
                    //Headless mode changes user agent so we need to force it to use "real" user agent
                    Args = new[]
                    {
                        "--user-agent=\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3882.0 Safari/537.36\""
                    }
                });

                _logger.Debug("Opening new page");
                var descriptionPage = await _browser.NewPageAsync();
                await descriptionPage.SetContentAsync("<h1>This is a browser of patreon downloader</h1>");

                _logger.Debug("Creating IWebBrowser");
                _browserWrapper = new WebBrowser(_browser);

                return _browserWrapper;
            }
            catch (PuppeteerException ex)
            {
                _logger.Fatal($"Browser communication error. Exception: {ex}");
                return null;
            }
        }

        /// <summary>
        ///     Initialize connection to remote browser
        /// </summary>
        /// <returns></returns>
        private async Task<IWebBrowser> ConnectToRemoteBrowser()
        {
            try
            {
                _logger.Debug("Connecting to remote browser");
                _browser = await Puppeteer.ConnectAsync(new ConnectOptions
                {
                    BrowserURL = _remoteBrowserAddress.ToString()
                });

                _logger.Debug("Opening new page");
                var descriptionPage = await _browser.NewPageAsync();
                await descriptionPage.SetContentAsync("<h1>This is a browser of patreon downloader</h1>");

                _logger.Debug("Creating IWebBrowser");
                _browserWrapper = new WebBrowser(_browser);

                return _browserWrapper;
            }
            catch (PuppeteerException ex)
            {
                _logger.Fatal($"Browser communication error. Exception: {ex}");
                return null;
            }
        }
    }
}