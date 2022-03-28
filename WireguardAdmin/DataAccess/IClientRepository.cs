using System.Threading.Tasks;

namespace WireguardAdmin.Models
{
    public interface IClientRepository
    {
        Task<NewUserModel> AddAsync(string username, string password);
    }
}