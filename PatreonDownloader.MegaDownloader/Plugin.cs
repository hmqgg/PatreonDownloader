using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using NLog;
using PatreonDownloader.Common.Interfaces.Plugins;
using PatreonDownloader.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PatreonDownloader.MegaDownloader
{
    public class Plugin : IPlugin
    {
        private static readonly Regex NewFormatRegex;
        private static readonly Regex OldFormatRegex;
        private static readonly MegaCredentials MegaCredentials;
        private static readonly MegaDownloader MegaDownloader;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private bool _overwriteFiles;

        static Plugin()
        {
            NewFormatRegex =
                new Regex(@"/(?<type>(file|folder))/(?<id>[^#]+)#(?<key>[a-zA-Z0-9_-]+)"); //Regex("(#F|#)![a-zA-Z0-9]{0,8}![a-zA-Z0-9_-]+");
            OldFormatRegex = new Regex(@"#(?<type>F?)!(?<id>[^!]+)!(?<key>[^$!\?]+)");

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mega_credentials.json");

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath, true, false)
                .Build();

            if (!File.Exists(configPath))
            {
                LogManager.GetCurrentClassLogger()
                    .Warn(
                        "!!!![MEGA]: mega_credentials.json not found, mega downloading will be limited! Refer to documentation for additional information. !!!!");
            }
            else
            {
                MegaCredentials = new MegaCredentials(configuration["email"], configuration["password"]);
            }

            try
            {
                MegaDownloader = new MegaDownloader(MegaCredentials);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger()
                    .Fatal(ex,
                        "!!!![MEGA]: Unable to initialize mega downloader, check email and password! No mega files will be downloaded in this session. !!!!");
            }
        }

        public string Name => "Mega.nz Downloader";
        public string Author => "Aleksey Tsutsey";
        public string ContactInformation => "https://github.com/Megalan/PatreonDownloader";

        public Task BeforeStart(bool overwriteFiles)
        {
            _overwriteFiles = overwriteFiles;
            return Task.CompletedTask;
        }

        public Task Download(CrawledUrl crawledUrl, string downloadDirectory)
        {
            if (MegaDownloader == null)
            {
                _logger.Fatal($"Mega downloader initialization failure (check credentials), {crawledUrl.Url} will not be downloaded!");
                return Task.CompletedTask;
            }

            try
            {
                var result = MegaDownloader.DownloadUrl(crawledUrl, downloadDirectory);

                if (result != MegaDownloadResult.Success)
                {
                    _logger.Error($"Error while downloading {crawledUrl.Url}! {result}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"MEGA DOWNLOAD EXCEPTION: {ex}");
            }

            return Task.CompletedTask;
        }

        public Task<List<string>> ExtractSupportedUrls(string htmlContents)
        {
            var retList = new List<string>();
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContents);
            var parseText = string.Join(" ", doc.DocumentNode.Descendants()
                .Where(n => !n.HasChildNodes && !string.IsNullOrWhiteSpace(n.InnerText))
                .Select(n => n.InnerText)); //first get a copy of text without all html tags
            parseText += doc.DocumentNode
                .InnerHtml; //now append a copy of this text with all html tags intact (otherwise we lose all <a href=... links)

            var matchesNewFormat = NewFormatRegex.Matches(parseText);

            var matchesOldFormat = OldFormatRegex.Matches(parseText);

            _logger.Debug($"Found NEW:{matchesNewFormat.Count}|OLD:{matchesOldFormat.Count} possible mega links in description");

            var megaUrls = new List<string>();

            foreach (Match match in matchesNewFormat)
            {
                _logger.Debug($"Parsing mega match new format {match.Value}");
                megaUrls.Add(
                    $"https://mega.nz/{match.Groups["type"].Value.Trim()}/{match.Groups["id"].Value.Trim()}#{match.Groups["key"].Value.Trim()}");
            }

            foreach (Match match in matchesOldFormat)
            {
                _logger.Debug($"Parsing mega match old format {match.Value}");
                megaUrls.Add(
                    $"https://mega.nz/#{match.Groups["type"].Value.Trim()}!{match.Groups["id"].Value.Trim()}!{match.Groups["key"].Value.Trim()}");
            }

            foreach (var url in megaUrls)
            {
                var sanitizedUrl = url.Split(' ')[0].Replace("&lt;wbr&gt;", "").Replace("&lt;/wbr&gt;", "");
                _logger.Debug($"Adding mega match {sanitizedUrl}");
                if (retList.Contains(sanitizedUrl))
                {
                    _logger.Debug($"Already parsed, skipping: {sanitizedUrl}");
                    continue;
                }

                retList.Add(sanitizedUrl);
            }

            return Task.FromResult(retList);
        }

        public Task<bool> IsSupportedUrl(string url)
        {
            var matchesNewFormat = NewFormatRegex.Matches(url);
            var matchesOldFormat = OldFormatRegex.Matches(url);

            return Task.FromResult(matchesOldFormat.Count > 0 || matchesNewFormat.Count > 0);
        }
    }
}
