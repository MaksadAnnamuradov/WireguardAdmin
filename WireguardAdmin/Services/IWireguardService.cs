using System.Threading.Tasks;
using WireguardAdmin.Models;

namespace WireguardAdmin.Services
{
    public interface IWireguardService
    {
        bool Check(string hash, string password, byte[] salt);
        Task GenereateNewClientConf(NewClientModel newClient);
        Task<string> GetClientPrivateKey(string clientName);
        Task<string> GetClientPublicKey(string clientName);
        Task<string> getStatus();
        Task Restart();
        Task UpdateServerFile(NewClientModel newClient);
    }
}