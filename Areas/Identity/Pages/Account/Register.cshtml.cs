#nullable disable

using EeD_BE_EeD.Models;
using EeD_BE_EeD.Services.ActivityLogger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;

namespace EeD_BE_EeD.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IActivityLogger _activityLogger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IActivityLogger activityLogger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _activityLogger = activityLogger;
        }

        public List<string> Countries { get; set; } = new()
        {
            "Jordan", "United States", "Canada", "United Kingdom", "Germany", "France", "Turkey", "UAE", "Saudi Arabia"
        };

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password")]
            public string ConfirmPassword { get; set; }

            [Required, StringLength(100)]
            public string FullName { get; set; }

            [Required]
            [RegularExpression(@"^009627[789]\d{7}$")]
            public string PhoneNumber { get; set; }

            public string Country { get; set; }
            public IFormFile ProfileImage { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
                    Response.Redirect("/Admin/Dashboard/Index");
                else
                    Response.Redirect("/User/User/UserHome");

                return;
            }

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var user = CreateUser();
            user.FullName = Input.FullName;
            user.PhoneNumber = Input.PhoneNumber;
            user.Country = Input.Country;

            if (Input.ProfileImage != null)
            {
                string folder = Path.Combine("wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfileImage.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Input.ProfileImage.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = "/uploads/profiles/" + fileName;
            }

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created successfully");
                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                await _activityLogger.LogAsync(user.Id, "Register", $"New user registered: {user.Email}");


                // ✅ Add default role
                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, false);

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
                    return LocalRedirect("/Admin/Dashboard/Index");

                return LocalRedirect("/User/User/UserHome");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try { return Activator.CreateInstance<ApplicationUser>(); }
            catch { throw new InvalidOperationException("Error creating ApplicationUser"); }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("Email not supported.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
