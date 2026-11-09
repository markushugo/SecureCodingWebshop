using Microsoft.AspNetCore.Mvc;
using Webshop.Library.Models;
using Webshop.Library.Services;

namespace Webshop.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                bool isValid = _userService.ValidateCredentials(model.Username, model.Password);

                if (isValid)
                {
                    // Simpel session-håndtering - i en rigtig app ville vi bruge cookies eller tokens
                    HttpContext.Session.SetString("UserEmail", model.Username);
                    return LocalRedirect(returnUrl ?? "/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Forkert brugernavn eller adgangskode.");
                    model.ErrorMessage = "Forkert brugernavn eller adgangskode.";
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}