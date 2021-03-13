﻿using Ninject.Modules;
using PatreonDownloader.PuppeteerEngine.Wrappers.Browser;

namespace PatreonDownloader.PuppeteerEngine
{
    public class PuppeteerEngineModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPuppeteerEngine>().To<PuppeteerEngine>().InSingletonScope();
            //Bind<ICookieRetriever>().To<PuppeteerCookieRetriever>();
            Bind<IWebBrowser>().To<WebBrowser>();
            Bind<IWebPage>().To<WebPage>();
            Bind<IWebRequest>().To<WebRequest>();
            Bind<IWebResponse>().To<WebResponse>();
        }
    }
}
