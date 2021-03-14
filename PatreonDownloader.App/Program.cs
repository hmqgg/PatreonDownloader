using CommandLine;
using NLog;
using PatreonDownloader.App.Models;
using PatreonDownloader.Engine;
using PatreonDownloader.Engine.Enums;
using PatreonDownloader.Engine.Events;
using PatreonDownloader.PuppeteerEngine;
using System;
using System.Threading.Tasks;

namespace PatreonDownloader.App
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Engine.PatreonDownloader _patreonDownloader;
        private static PuppeteerCookieRetriever _cookieRetriever;
        private static int _filesDownloaded;

        private static async Task Main(string[] args)
        {
            NLogManager.ReconfigureNLog();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

            string url = null;
            var headlessBrowser = true;
            string remoteBrowserAddress = null;

            PatreonDownloaderSettings settings = null;
            parserResult.WithParsed(options =>
            {
                url = options.Url;
                headlessBrowser = !options.NoHeadless;
                remoteBrowserAddress = options.RemoteBrowserAddress;
                settings = new PatreonDownloaderSettings
                {
                    SaveAvatarAndCover = options.SaveAvatarAndCover,
                    SaveDescriptions = options.SaveDescriptions,
                    SaveEmbeds = options.SaveEmbeds,
                    SaveJson = options.SaveJson,
                    UpgradeId = options.UpgradeId,
                    TitleInclude = options.TitleInclude,
                    TitleExclude = options.TitleExclude,
                    FilenameInclude = options.FilenameInclude,
                    FilenameExclude = options.FilenameExclude,
                    PostIds = options.PostIds?.Split(';'),
                    AttachmentOnly = options.AttachmentOnly,
                    NoExternal = options.NoExternal,
                    DateAfter = DateTime.Parse(options.DateAfter),
                    DownloadDirectory = options.DownloadDirectory,
                    OverwriteFiles = options.OverwriteFiles
                };
                NLogManager.ReconfigureNLog(options.Verbose);
            });

            if (string.IsNullOrEmpty(url) || settings == null)
            {
                return;
            }

            Uri remoteBrowserUri = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(remoteBrowserAddress))
                {
                    remoteBrowserUri = new Uri(remoteBrowserAddress);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"Invalid remote browser address: {remoteBrowserAddress}");
                Environment.Exit(0);
                return;
            }

            try
            {
                await RunPatreonDownloader(url, headlessBrowser, remoteBrowserUri, settings);
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Fatal error, application will be closed: {ex}");
                Environment.Exit(0);
            }
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Logger.Info("Cancellation requested");
            Cleanup();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Debug("Entered process exit");
            Cleanup();
        }

        private static void Cleanup()
        {
            Logger.Debug("Cleanup called");
            if (_patreonDownloader != null)
            {
                Logger.Debug("Disposing downloader...");
                try
                {
                    _patreonDownloader.Dispose();
                    _patreonDownloader = null;
                }
                catch (Exception ex)
                {
                    Logger.Fatal($"Error during patreon downloader disposal! Exception: {ex}");
                }
            }

            if (_cookieRetriever != null)
            {
                Logger.Debug("Disposing cookie retriever...");
                try
                {
                    _cookieRetriever.Dispose();
                    _cookieRetriever = null;
                }
                catch (Exception ex)
                {
                    Logger.Fatal($"Error during cookie retriever disposal! Exception: {ex}");
                }
            }
        }

        private static async Task RunPatreonDownloader(string url, bool headlessBrowser, Uri remoteBrowserAddress, PatreonDownloaderSettings settings)
        {
            if (remoteBrowserAddress == null)
            {
                _cookieRetriever = new PuppeteerCookieRetriever(headlessBrowser);
            }
            else
            {
                _cookieRetriever = new PuppeteerCookieRetriever(remoteBrowserAddress);
            }

            Logger.Info("Retrieving cookies...");
            var cookieContainer = await _cookieRetriever.RetrieveCookies();
            if (cookieContainer == null)
            {
                throw new Exception("Unable to retrieve cookies");
            }

            _cookieRetriever.Dispose();
            _cookieRetriever = null;

            await Task.Delay(1000); //wait for PuppeteerCookieRetriever to close the browser

            _patreonDownloader = remoteBrowserAddress == null
                ? new Engine.PatreonDownloader(cookieContainer, headlessBrowser)
                : new Engine.PatreonDownloader(cookieContainer, remoteBrowserAddress);

            _filesDownloaded = 0;

            _patreonDownloader.StatusChanged += PatreonDownloaderOnStatusChanged;
            _patreonDownloader.PostCrawlStart += PatreonDownloaderOnPostCrawlStart;
            //_patreonDownloader.PostCrawlEnd += PatreonDownloaderOnPostCrawlEnd;
            _patreonDownloader.NewCrawledUrl += PatreonDownloaderOnNewCrawledUrl;
            _patreonDownloader.CrawlerMessage += PatreonDownloaderOnCrawlerMessage;
            _patreonDownloader.FileDownloaded += PatreonDownloaderOnFileDownloaded;
            await _patreonDownloader.Download(url, settings);

            _patreonDownloader.Dispose();
            _patreonDownloader = null;
        }

        private static void PatreonDownloaderOnCrawlerMessage(object sender, CrawlerMessageEventArgs e)
        {
            switch (e.MessageType)
            {
                case CrawlerMessageType.Info:
                    Logger.Info(e.Message);
                    break;
                case CrawlerMessageType.Warning:
                    Logger.Warn(e.Message);
                    break;
                case CrawlerMessageType.Error:
                    Logger.Error(e.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void PatreonDownloaderOnNewCrawledUrl(object sender, NewCrawledUrlEventArgs e)
        {
            Logger.Info($"  + {e.CrawledUrl.UrlTypeAsFriendlyString}: {e.CrawledUrl.Url}");
        }

        private static void PatreonDownloaderOnPostCrawlEnd(object sender, PostCrawlEventArgs e)
        {
            /*if(!e.Success)
                _logger.Error($"Post cannot be parsed: {e.ErrorMessage}");*/
            //_logger.Info(e.Success ? "✓" : "✗");
        }

        private static void PatreonDownloaderOnPostCrawlStart(object sender, PostCrawlEventArgs e)
        {
            Logger.Info($"-> {e.PostId}");
        }

        private static void PatreonDownloaderOnFileDownloaded(object sender, FileDownloadedEventArgs e)
        {
            _filesDownloaded++;
            if (e.Success)
            {
                Logger.Info($"Downloaded {_filesDownloaded}/{e.TotalFiles}: {e.Url}");
            }
            else
            {
                Logger.Error($"Failed to download {e.Url}: {e.ErrorMessage}");
            }
        }

        private static void PatreonDownloaderOnStatusChanged(object sender, DownloaderStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case DownloaderStatus.Ready:
                    break;
                case DownloaderStatus.Initialization:
                    Logger.Info("Preparing to download...");
                    break;
                case DownloaderStatus.RetrievingCampaignInformation:
                    Logger.Info("Retrieving campaign information...");
                    break;
                case DownloaderStatus.Crawling:
                    Logger.Info("Crawling...");
                    break;
                case DownloaderStatus.Downloading:
                    Logger.Info("Downloading...");
                    break;
                case DownloaderStatus.Done:
                    Logger.Info("Finished");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
