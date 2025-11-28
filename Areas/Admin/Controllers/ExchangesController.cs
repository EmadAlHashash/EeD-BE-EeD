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
    public class ExchangesController : Controller
    {
 
        private readonly ApplicationDbContext _db;

        public ExchangesController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index(
           string? status,
           string? userName,
           DateTime? from,
           DateTime? to,
           int page = 1,
           int pageSize = 10)
        {
            var query = _db.Exchanges
                .Include(e => e.Requester)
                .Include(e => e.Provider)
                .Include(e => e.RequestedService)
                .Include(e => e.OfferedService)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status.ToString() == status);

            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(e =>
                    (e.Requester != null && e.Requester.FullName.Contains(userName)) ||
                    (e.Provider != null && e.Provider.FullName.Contains(userName)));

            if (from.HasValue)
                query = query.Where(e => e.RequestDate >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.RequestDate <= to.Value);

            // Stats
            var total = await query.CountAsync();
            var pending = await query.CountAsync(e => e.Status == ExchangeStatus.Pending);
            var completed = await query.CountAsync(e => e.Status == ExchangeStatus.Completed);
            var cancelled = await query.CountAsync(e => e.Status == ExchangeStatus.Cancelled);

            // Pagination
            var skip = (page - 1) * pageSize;

            var exchanges = await query
                .OrderByDescending(e => e.RequestDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var mapped = exchanges.Select(e => new ExchangeVm
            {
                Id = e.Id,
                RequesterName = e.Requester?.FullName ?? "Unknown",
                ProviderName = e.Provider?.FullName ?? "Unknown",
                RequestedServiceTitle = e.RequestedService?.Title ?? "-",
                OfferedServiceTitle = e.OfferedService?.Title ?? "-",
                Status = e.Status.ToString(),
                RequestDate = e.RequestDate,
                CompletionDate = e.CompletionDate
            }).ToList();

            var vm = new ExchangeManagementViewModel
            {
                Exchanges = mapped,

                Status = status,
                UserName = userName,
                From = from,
                To = to,

                AvailableStatuses = Enum.GetNames(typeof(ExchangeStatus)).ToList(),

                TotalExchanges = total,
                PendingCount = pending,
                CompletedCount = completed,
                CancelledCount = cancelled
            };

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = total;

            return View(vm);
        }
    }
}
