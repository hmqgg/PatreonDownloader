﻿using System.Collections.Generic;
using PatreonDownloader.Interfaces.Models;

namespace PatreonDownloader.Engine.Models
{
    /// <summary>
    ///     Represents one crawled page with all results and link to the next page
    /// </summary>
    internal struct ParsingResult
    {
        public List<CrawledUrl> Entries;
        public string NextPage;
    }
}
