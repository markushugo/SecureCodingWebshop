namespace Webshop.Library.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime AttemptTime { get; set; }
        public bool Success { get; set; }
    }
}