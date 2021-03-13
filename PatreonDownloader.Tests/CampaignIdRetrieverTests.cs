using System;
using System.Threading.Tasks;
using Moq;
using PatreonDownloader.Engine;
using PatreonDownloader.Engine.Exceptions;
using PatreonDownloader.Engine.Stages.Crawling;
using PatreonDownloader.Tests.Resources;
using Xunit;

namespace PatreonDownloader.Tests
{
    public class CampaignIdRetrieverTests
    {
        [Fact]
        public async void RetrieveCampaignId_ValidResponse_ReturnsCorrectId()
        {
            var webDownloaderMock = new Mock<IWebDownloader>(MockBehavior.Strict);
            webDownloaderMock.Setup(x => x.DownloadString(It.IsAny<string>()))
                .ReturnsAsync(EmbeddedFileReader.ReadEmbeddedFile<CampaignIdRetrieverTests>("CampaignIdRetriever.ValidResponse.json"));

            var campaignIdRetriever = new CampaignIdRetriever(webDownloaderMock.Object);

            var campaignId = await campaignIdRetriever.RetrieveCampaignId("testurl");

            Assert.Equal(3216549870, campaignId);
        }

        [Fact]
        public async Task RetrieveCampaignId_PatreonDownloaderException_ThrowsException()
        {
            var webDownloaderMock = new Mock<IWebDownloader>(MockBehavior.Strict);

            webDownloaderMock.Setup(x => x.DownloadString(It.IsAny<string>()))
                .Throws<PatreonDownloaderException>();

            var campaignIdRetriever = new CampaignIdRetriever(webDownloaderMock.Object);

            await Assert.ThrowsAsync<PatreonDownloaderException>(async () => await campaignIdRetriever.RetrieveCampaignId("testurl"));
        }

        [Fact]
        public async void RetrieveCampaignId_RequestUrlDoesNotContainId_ReturnsMinusOne()
        {
            var webDownloaderMock = new Mock<IWebDownloader>(MockBehavior.Strict);
            webDownloaderMock.Setup(x => x.DownloadString(It.IsAny<string>()))
                .ReturnsAsync(EmbeddedFileReader.ReadEmbeddedFile<CampaignIdRetrieverTests>("CampaignIdRetriever.InvalidResponse.json"));

            var campaignIdRetriever = new CampaignIdRetriever(webDownloaderMock.Object);

            var campaignId = await campaignIdRetriever.RetrieveCampaignId("testurl");

            Assert.Equal(-1, campaignId);
        }

        [Fact]
        public async void RetrieveCampaignId_UrlIsNull_ThrowsArgumentException()
        {
            var webDownloaderMock = new Mock<IWebDownloader>(MockBehavior.Strict);

            var campaignIdRetriever = new CampaignIdRetriever(webDownloaderMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await campaignIdRetriever.RetrieveCampaignId(null));
        }
    }
}
