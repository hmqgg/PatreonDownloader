using System;
using System.Threading.Tasks;
using PatreonDownloader.PuppeteerEngine.Wrappers.Browser;

namespace PatreonDownloader.PuppeteerEngine
{
    public interface IPuppeteerEngine : IDisposable
    {
        bool IsHeadless { get; }
        Task<IWebBrowser> GetBrowser();
        Task CloseBrowser();
    }
}
