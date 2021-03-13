using System;
using PatreonDownloader.Interfaces.Models;

namespace PatreonDownloader.Engine.Events
{
    public sealed class NewCrawledUrlEventArgs : EventArgs
    {
        public NewCrawledUrlEventArgs(CrawledUrl crawledUrl)
        {
            CrawledUrl = crawledUrl ?? throw new ArgumentNullException(nameof(crawledUrl));
        }

        public CrawledUrl CrawledUrl { get; }
    }
}
