using NLog;
using PatreonDownloader.Common.Exceptions;
using PatreonDownloader.Common.Interfaces.Plugins;
using PatreonDownloader.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PatreonDownloader.GoogleDriveDownloader
{
    public sealed class Plugin : IPlugin
    {
        private static readonly Regex GoogleDriveRegex;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly GoogleDriveEngine Engine;

        private bool _overwriteFiles;

        static Plugin()
        {
            if (!File.Exists("gd_credentials.json"))
            {
                LogManager.GetCurrentClassLogger()
                    .Fatal(
                        "!!!![GOOGLE DRIVE]: gd_credentials.json not found, google drive files will not be downloaded! Refer to documentation for additional information. !!!!");
            }

            GoogleDriveRegex =
                new Regex(
                    "https:\\/\\/drive\\.google\\.com\\/(?:file\\/d\\/|open\\?id\\=|drive\\/folders\\/|folderview\\?id=|drive\\/u\\/[0-9]+\\/folders\\/)([A-Za-z0-9_-]+)");
            Engine = new GoogleDriveEngine();
        }

        public string Name => "Google Drive Downloader";
        public string Author => "Aleksey Tsutsey";
        public string ContactInformation => "https://github.com/Megalan/PatreonDownloader";

        public Task BeforeStart(bool overwriteFiles)
        {
            _overwriteFiles = overwriteFiles;
            return Task.CompletedTask;
        }

        public Task<bool> IsSupportedUrl(string url)
        {
            var match = GoogleDriveRegex.Match(url);
            return Task.FromResult(match.Success);
        }

        public Task Download(CrawledUrl crawledUrl, string downloadDirectory)
        {
            Logger.Debug($"Received new url: {crawledUrl.Url}, download dir: {downloadDirectory}");

            var match = GoogleDriveRegex.Match(crawledUrl.Url);
            if (!match.Success)
            {
                Logger.Error($"Unable to parse google drive url: {crawledUrl.Url}");
                throw new DownloadException($"Unable to parse google drive url: {crawledUrl.Url}");
            }

            var id = match.Groups[1].Value;

            var downloadPath = Path.Combine(downloadDirectory,
                $"{crawledUrl.PostId}_{id.Substring(id.Length - 6, 5)}_gd_").TrimEnd('/', '\\');

            Logger.Debug($"Retrieved id: {id}, download path: {downloadPath}");

            try
            {
                Engine.Download(id, downloadPath, _overwriteFiles);
            }
            catch (Exception ex)
            {
                Logger.Error("GOOGLE DRIVE ERROR: " + ex);
                throw new DownloadException($"Unable to download {crawledUrl.Url}", ex);
            }

            return Task.CompletedTask;
        }

        public Task<List<string>> ExtractSupportedUrls(string htmlContents)
        {
            //Let default plugin do this
            return Task.FromResult<List<string>>(null);
        }
    }
}
