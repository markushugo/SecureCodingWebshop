namespace Webshop.UI.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public bool RememberMe { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}