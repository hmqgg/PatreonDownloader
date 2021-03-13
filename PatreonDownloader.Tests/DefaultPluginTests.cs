using System.Threading.Tasks;
using Moq;
using PatreonDownloader.Common.Interfaces.Plugins;
using PatreonDownloader.Engine;
using PatreonDownloader.Engine.Helpers;
using PatreonDownloader.Interfaces.Models;
using Xunit;

namespace PatreonDownloader.Tests
{
    public class DefaultPluginTests
    {
        [Fact]
        public async void Download_MultipleFilesWithTheSameName_RenamesFiles()
        {
            string moqPathPassed = null;
            var webDownloaderMock = new Mock<IWebDownloader>(MockBehavior.Strict);
            webDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), false))
                .Returns(Task.CompletedTask)
                .Callback<string, string>((url, path) => { moqPathPassed = path; });

            var remoteFilenameRetrieverMock = new Mock<IRemoteFilenameRetriever>(MockBehavior.Strict);
            remoteFilenameRetrieverMock.Setup(x => x.RetrieveRemoteFileName(It.IsAny<string>()))
                .ReturnsAsync("1.png");

            IPlugin defaultPlugin = new DefaultPlugin(webDownloaderMock.Object, remoteFilenameRetrieverMock.Object);

            //Test patreon renaming
            var crawledUrl = new CrawledUrl();
            crawledUrl.PostId = 123456;
            crawledUrl.Url =
                "https://c10.patreonusercontent.com/3/asdaslifdh2321hdsfosdfs%3D/patreon-media/p/post/12345678/4ds697ecx1s475f6er9v2fd4s7sa65h6/1.png?token-time=1234567890&token-hash=pihskdKHrkhsk7223hhdsadsdsadafdslkfherhdiun%3D";
            crawledUrl.Filename = null;
            crawledUrl.UrlType = CrawledUrlType.PostFile;

            await defaultPlugin.BeforeStart(false);
            await defaultPlugin.Download(crawledUrl, "c:\\testpath");
            var passedPath1 = moqPathPassed;

            crawledUrl.Url =
                "https://c10.patreonusercontent.com/3/asdaslifdh2321hdsfosdfs%3D/patreon-media/p/post/12345678/xfsadasdhahd234e325dhsfkshdkfhas/1.png?token-time=1234567890&token-hash=pihskdKHrkhsk7223hhdsadsdsadafdslkfherhdiun%3D";
            await defaultPlugin.Download(crawledUrl, "c:\\testpath");
            var passedPath2 = moqPathPassed;

            crawledUrl.Url =
                "https://c10.patreonusercontent.com/3/asdaslifdh2321hdsfosdfs%3D/patreon-media/p/post/12345678/cvvmhjkfghjoitupk23423r54hdsisds/1.png?token-time=1234567890&token-hash=pihskdKHrkhsk7223hhdsadsdsadafdslkfherhdiun%3D";
            await defaultPlugin.Download(crawledUrl, "c:\\testpath");
            var passedPath3 = moqPathPassed;

            Assert.Equal("c:\\testpath\\123456_post_1.png", passedPath1);
            Assert.Equal("c:\\testpath\\123456_post_1_xfsadasdhahd234e325dhsfkshdkfhas.png", passedPath2);
            Assert.Equal("c:\\testpath\\123456_post_1_cvvmhjkfghjoitupk23423r54hdsisds.png", passedPath3);

            //test external renaming
            crawledUrl.UrlType = CrawledUrlType.ExternalUrl;
            crawledUrl.Filename = "untitled.jpeg";
            crawledUrl.Url = "https://example.com/untitled.jpeg";

            await defaultPlugin.Download(crawledUrl, "c:\\testpath");
            var passedPath4 = moqPathPassed;

            await defaultPlugin.Download(crawledUrl, "c:\\testpath");
            var passedPath5 = moqPathPassed;

            await defaultPlugin.Download(crawledUrl, "c:\\testpath");
            var passedPath6 = moqPathPassed;

            Assert.Equal("c:\\testpath\\123456_external_untitled.jpeg", passedPath4);
            Assert.Equal("c:\\testpath\\123456_external_untitled_2.jpeg", passedPath5);
            Assert.Equal("c:\\testpath\\123456_external_untitled_3.jpeg", passedPath6);
        }
    }
}
