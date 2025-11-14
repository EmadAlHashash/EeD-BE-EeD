using EeD_BE_EeD.Areas.Admin.ViewModels;
using EeD_BE_EeD.Data;
using EeD_BE_EeD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EeD_BE_EeD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DashboardUserController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db; // الوصول للداتا بيس
            _userManager = userManager; // الوصول لمعلومات المستخدم + الأدوار
            _env = env;
        }

        public async Task<IActionResult> Index(string? search, string? status, string? role, int page = 1)
        {
            int pageSize = 10;
            // عدد المستخدمين في كل صفحة

            var query = _db.Users.AsNoTracking().OrderBy(u => u.FullName).AsQueryable();
            // سحب المستخدمين من الداتا بيس بشكل مرتب حسب الاسم

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => (u.FullName != null && u.FullName.Contains(s)) || (u.Email != null && u.Email.Contains(s)));
            }

            // Execute query to list before role filter (needs roles per user)
            var usersAll = await query.ToListAsync();
            var currentUserId = _userManager.GetUserId(User);

            var items = new List<UserListItemViewModel>();

            foreach (var u in usersAll)
            {
                var roles = await _userManager.GetRolesAsync(u);
                string roleName = roles.FirstOrDefault() ?? "User";

                // الحالة حسب المطلوب: المحظور = Banned، المستخدم الحالي = Active، غيره = Inactive
                string calcStatus;
                if (string.Equals(u.Status, "Banned", StringComparison.OrdinalIgnoreCase))
                {
                    calcStatus = "Banned";
                }
                else if (!string.IsNullOrEmpty(currentUserId) && u.Id == currentUserId)
                {
                    calcStatus = "Active";
                }
                else
                {
                    calcStatus = "Inactive";
                }

                items.Add(new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName ?? "Unknown",
                    Email = u.Email!,
                    Role = roleName,
                    Status = calcStatus,
                    AvatarUrl = u.ProfilePictureUrl
                });
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                items = items.Where(x => string.Equals(x.Role, role, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                items = items.Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int totalUsers = items.Count;
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pageItems = items
                .OrderBy(x => x.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new UsersListViewModel
            {
                Users = pageItems,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            // pass current filters to view
            ViewData["search"] = search ?? string.Empty;
            ViewData["status"] = status ?? string.Empty;
            ViewData["role"] = role ?? string.Empty;
            ViewData["totalUsers"] = totalUsers;

            return View(vm);
        }
    }
}
