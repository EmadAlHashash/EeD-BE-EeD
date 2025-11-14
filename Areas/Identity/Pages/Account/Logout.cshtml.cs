using EeD_BE_EeD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace EeD_BE_EeD.Areas.Identity.Pages.Account
{
    // Silent override of Identity Logout page: always redirect to home and do not show any UI
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            // Do not log out here; force redirect to home to prevent Identity UI messages
            return Redirect("/");
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Mark user as Inactive on logout
                    user.Status = "Inactive";
                    await _userManager.UpdateAsync(user);
                }
                await _signInManager.SignOutAsync();
            }
            _logger.LogInformation("Silent logout page redirect invoked.");
            return Redirect("/");
        }
    }
}
