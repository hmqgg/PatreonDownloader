using System.Threading.Tasks;

namespace PatreonDownloader.Engine.Helpers
{
    internal interface IRemoteFilenameRetriever
    {
        Task<string> RetrieveRemoteFileName(string url);
    }
}
