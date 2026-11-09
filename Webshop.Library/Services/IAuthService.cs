using System.Threading.Tasks;

namespace Webshop.Library.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
    }
}