using System.Threading.Tasks;
using Webshop.Library.Models;

namespace Webshop.Library.IRepos
{
    public interface IUserRepo
    {
        Task<AppUser> GetByEmailAsync(string email);
        Task UpdateLoginAttemptsAsync(AppUser user);
    }
}