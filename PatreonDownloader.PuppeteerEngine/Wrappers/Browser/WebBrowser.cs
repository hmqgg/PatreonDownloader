﻿using System;
using System.Threading.Tasks;

namespace PatreonDownloader.PuppeteerEngine.Wrappers.Browser
{
    /// <summary>
    ///     This class is a wrapper around a Puppeteer Sharp's browser object used to implement proper dependency injection
    ///     mechanism
    ///     It should copy any used puppeteer sharp's method definitions for ease of code maintenance
    /// </summary>
    public sealed class WebBrowser : IWebBrowser
    {
        private readonly PuppeteerSharp.Browser _browser;

        public WebBrowser(PuppeteerSharp.Browser browser)
        {
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        public async Task<IWebPage> NewPageAsync()
        {
            IWebPage page = new WebPage(await _browser.NewPageAsync());

            return page;
        }
    }
}
