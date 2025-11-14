using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EeD_BE_EeD.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User")]

    public class UserController : Controller
    {
        public IActionResult UserHome()
        {
            return View();
        }
    }
}
