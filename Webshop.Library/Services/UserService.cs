using System;
using Webshop.Library.Models;

namespace Webshop.Library.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public bool ValidateCredentials(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            // Simpel validering - i en rigtig app ville vi bruge hashing og salt
            bool isValid = email == "admin@example.com" && password == "Admin123!";

            if (isValid)
            {
                user.FailedLoginAttempts = 0;
                _context.SaveChanges();
                return true;
            }
            else
            {
                user.FailedLoginAttempts++;
                user.IsLockedOut = user.FailedLoginAttempts >= 3;
                _context.SaveChanges();
                return false;
            }
        }
    }
}