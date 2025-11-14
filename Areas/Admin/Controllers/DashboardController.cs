using EeD_BE_EeD.Areas.Admin.ViewModels;
using EeD_BE_EeD.Data;
using EeD_BE_EeD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EeD_BE_EeD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // ---- 🟩 البطاقات العلوية ----
            var totalUsers = await _db.Users.AsNoTracking().CountAsync();

            var activeServices = await _db.Set<Service>()
                .AsNoTracking()
                .CountAsync(s => s.IsActive);

            var pendingExchanges = await _db.Set<Exchange>()
                .AsNoTracking()
                .CountAsync(e => e.Status == ExchangeStatus.Pending);

            var newReviewsLast7Days = await _db.Set<Review>()
                .AsNoTracking()
                .CountAsync(r => r.DatePosted >= DateTime.UtcNow.AddDays(-7));

            // ---- 🟦 الشارت: عدد المستخدمين الجدد لكل شهر ----
            // EF Core ما بترجم string.Format، فبنجهزها بـ C# بعد الـToList
            var usersPerMonthRaw = await _db.ActivityLogs
                .AsNoTracking()
                .Where(a => a.Action == "Register")
                .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Take(12)
                .ToListAsync();

            var newUsersPerMonth = usersPerMonthRaw
                .Select(x => new MonthlyCount
                {
                    Month = $"{x.Year:D4}-{x.Month:D2}",
                    Count = x.Count
                })
                .ToList();

            // ---- 🟨 الشارت: عدد الخدمات الفعالة لكل تصنيف ----
            var servicesPerCategory = await _db.Set<Service>()
                .AsNoTracking()
                .Where(s => s.IsActive)
                .GroupBy(s => s.Category.Name)
                .Select(g => new CategoryCount
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // ---- 🟧 الأنشطة الأخيرة ----
            var recentActivities = await _db.ActivityLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Include(a => a.User)
                .Take(15)
                .Select(a => new ActivityItemViewModel
                {
                    Action = a.Action,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
            var currentUser = await _db.Set<ApplicationUser>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            // ---- 🟪 تعبئة الـ ViewModel ----
            var vm = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveServices = activeServices,
                PendingExchanges = pendingExchanges,
                NewReviews = newReviewsLast7Days,
                NewUsersPerMonth = newUsersPerMonth,
                ActiveServicesPerCategory = servicesPerCategory,
                RecentActivities = recentActivities,
                CurrentUserFullName = currentUser?.FullName ?? "User",

            };

            return View(vm);
        }
    }
}
