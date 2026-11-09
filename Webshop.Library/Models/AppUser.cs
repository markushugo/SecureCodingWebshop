namespace Webshop.Library.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLockedOut { get; set; }
    }
}