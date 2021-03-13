using System.Net;
using System.Threading.Tasks;

namespace PatreonDownloader.Engine.Stages.Initialization
{
    internal interface ICookieValidator
    {
        Task ValidateCookies(CookieContainer cookieContainer);
    }
}
