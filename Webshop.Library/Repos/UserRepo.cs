using System.Threading.Tasks;
using Webshop.Library.IRepos;
using Webshop.Library.Models;

namespace Webshop.Library.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;

        public UserRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser> GetByEmailAsync(string email)
        {
            return await _context.Users.FindAsync(email);
        }

        public async Task UpdateLoginAttemptsAsync(AppUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}