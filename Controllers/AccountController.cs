using EeD_BE_EeD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace EeD_BE_EeD.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("/Account/Logout")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    // Sign out of the default Identity application scheme
                    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    await _signInManager.SignOutAsync();
                }

                // Clear session if enabled (ignore if not configured)
                try { HttpContext.Session.Clear(); } catch { }

                // Robust cookie deletion (ensure Path="/")
                var cookieOptions = new CookieOptions { Path = "/" };
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie, cookieOptions);
                }

                _logger.LogInformation("User logged out (if authenticated). All cookies cleared.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while logging out");
            }

            // Always navigate to home
            return Redirect("/");
        }
    }
}
