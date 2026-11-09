using System.Threading.Tasks;

namespace Webshop.Library.Services
{
    public class AuthService : IAuthService
    {
        public async Task<bool> LoginAsync(string username, string password)
        {
            return await Task.FromResult(true);
        }

        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
        }
    }
}