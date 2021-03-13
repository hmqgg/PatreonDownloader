using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using PatreonDownloader.Engine.Events;
using PatreonDownloader.Engine.Exceptions;
using PatreonDownloader.Engine.Helpers;
using PatreonDownloader.Interfaces.Models;

namespace PatreonDownloader.Engine.Stages.Downloading
{
    internal sealed class DownloadManager : IDownloadManager
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IPluginManager _pluginManager;

        public DownloadManager(IPluginManager pluginManager)
        {
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        }

        public event EventHandler<FileDownloadedEventArgs> FileDownloaded;

        public async Task Download(List<CrawledUrl> crawledUrls, string downloadDirectory)
        {
            if (crawledUrls == null)
            {
                throw new ArgumentNullException(nameof(crawledUrls));
            }

            if (string.IsNullOrEmpty(downloadDirectory))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(downloadDirectory));
            }

            for (var i = 0; i < crawledUrls.Count; i++)
            {
                var entry = crawledUrls[i];

                if (!UrlChecker.IsValidUrl(entry.Url))
                {
                    _logger.Error($"[{entry.PostId}] Invalid or blacklisted external entry of type {entry.UrlType}: {entry.Url}");
                    continue;
                }

                _logger.Debug($"Downloading {i + 1}/{crawledUrls.Count}: {entry.Url}");

                _logger.Debug($"{entry.Url} is {entry.UrlType}");

                try
                {
                    await _pluginManager.DownloadCrawledUrl(entry, downloadDirectory);
                    OnFileDownloaded(new FileDownloadedEventArgs(entry.Url, crawledUrls.Count));
                }
                catch (DownloadException ex)
                {
                    var logMessage = $"Error while downloading {entry.Url}: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        logMessage += $". Inner Exception: {ex.InnerException}";
                    }

                    _logger.Error(logMessage);
                    OnFileDownloaded(new FileDownloadedEventArgs(entry.Url, crawledUrls.Count, false, logMessage));
                }
                catch (Exception ex)
                {
                    throw new PatreonDownloaderException($"Error while downloading {entry.Url}: {ex.Message}", ex);
                }
            }
        }

        private void OnFileDownloaded(FileDownloadedEventArgs e)
        {
            var handler = FileDownloaded;

            handler?.Invoke(this, e);
        }
    }
}
