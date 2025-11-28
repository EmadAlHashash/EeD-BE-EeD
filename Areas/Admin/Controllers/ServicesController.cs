using EeD_BE_EeD.Areas.Admin.ViewModels;
using EeD_BE_EeD.Data;
using EeD_BE_EeD.Models;
using EeD_BE_EeD.Services.ActivityLogger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EeD_BE_EeD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IActivityLogger _logger;

        public ServicesController(ApplicationDbContext db, IActivityLogger logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================
        //   LIST + FILTERS
        // ============================
        public async Task<IActionResult> Index(int? categoryId, ServiceStatus? status, string? city, int page = 1, int pageSize = 10)
        {
            var query = _db.Services
                .Include(s => s.Owner)
                .Include(s => s.Category)
                .AsNoTracking()
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(s => s.CategoryId == categoryId.Value);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(s => s.Owner != null && s.Owner.Country == city);

            var total = await query.CountAsync();

            var services = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new ServiceManagementViewModel
            {
                Services = services.Select(s => new ServiceVm
                {
                    Id = s.Id,
                    Title = s.Title ?? string.Empty,
                    Category = s.Category?.Name ?? string.Empty,
                    UserName = s.Owner?.FullName ?? s.Owner?.UserName ?? string.Empty,
                    ExchangeSkill = s.DesiredSkillOrService,
                    CreatedAt = s.CreatedAt,
                    Status = s.Status
                }).ToList(),

                Categories = (await _db.Categories.AsNoTracking().ToListAsync()).Select(c => new CategoryVm
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList(),

                CategoryId = categoryId,
                Status = status,
                City = city,
                ServicesPage = page,
                PageSize = pageSize,
                ServicesTotalCount = total,
                CategoriesTotalCount = await _db.Categories.CountAsync(),
                AvailableCities = await _db.Users.Select(u => u.Country).Where(c => c != null).Distinct().ToListAsync() ?? new List<string>()
            };

            return View(vm);
        }

        // ----- CATEGORY MANAGEMENT -----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory()
        {
            CategoryVm? model = null;

            // Try to bind JSON body
            try
            {
                var contentType = Request.ContentType ?? string.Empty;
                if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        model = System.Text.Json.JsonSerializer.Deserialize<CategoryVm>(body, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
            }
            catch
            {
                // ignore json parse errors and try form
            }

            // If not JSON, try form values (normal form submit)
            if (model == null && Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                model = new CategoryVm
                {
                    Id = int.TryParse(form["Id"], out var idVal) ? idVal : 0,
                    Name = form["Name"].FirstOrDefault(),
                    Description = form["Description"].FirstOrDefault(),
                    IsActive = model?.IsActive ?? (form.ContainsKey("IsActive") && (form["IsActive"].FirstOrDefault() == "true" || form["IsActive"].FirstOrDefault() == "on" || form["IsActive"].FirstOrDefault() == "checked"))
                };
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Invalid category data" });

            var cat = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description,
                IsActive = model.IsActive
            };

            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();

            await _logger.LogAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), "CreateCategory", $"Category '{cat.Name}' created (id={cat.Id})");

            return Json(new { success = true, message = "Category created", id = cat.Id, name = cat.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory([FromBody] CategoryVm model)
        {
            if (model == null || model.Id <= 0)
                return Json(new { success = false, message = "Invalid category data" });

            var cat = await _db.Categories.FindAsync(model.Id);
            if (cat == null) return Json(new { success = false, message = "Category not found" });

            cat.Name = model.Name ?? cat.Name;
            cat.Description = model.Description;
            cat.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            await _logger.LogAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), "EditCategory", $"Category '{cat.Name}' (id={cat.Id}) edited");

            return Json(new { success = true, message = "Category updated" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory([FromBody] CategoryVm model)
        {
            if (model == null || model.Id <= 0)
                return Json(new { success = false, message = "Invalid category id" });

            var cat = await _db.Categories.Include(c => c.Services).FirstOrDefaultAsync(c => c.Id == model.Id);
            if (cat == null) return Json(new { success = false, message = "Category not found" });

            // prevent delete if services exist
            var hasServices = await _db.Services.AnyAsync(s => s.CategoryId == cat.Id);
            if (hasServices)
                return Json(new { success = false, message = "Cannot delete category with existing services" });

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();

            await _logger.LogAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), "DeleteCategory", $"Category '{cat.Name}' (id={cat.Id}) deleted");

            return Json(new { success = true, message = "Category deleted" });
        }

        // ----- SERVICES MANAGEMENT -----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve([FromForm] int? id)
        {
            int? resolvedId = id;

            // If no form id, try query
            if (resolvedId == null && Request.Query.ContainsKey("id") && int.TryParse(Request.Query["id"], out var qid))
                resolvedId = qid;

            // If still null and JSON body, try read JSON manually
            if (resolvedId == null)
            {
                try
                {
                    var contentType = Request.ContentType ?? string.Empty;
                    if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        using var reader = new StreamReader(Request.Body);
                        var body = await reader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            try
                            {
                                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(body, new System.Text.Json.JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                                if (dict != null && dict.TryGetValue("id", out var v)) resolvedId = v;
                            }
                            catch
                            {
                                if (int.TryParse(body.Trim(), out var single)) resolvedId = single;
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            if (resolvedId == null)
            {
                var info = new
                {
                    contentType = Request.ContentType,
                    queryId = Request.Query.ContainsKey("id") ? Request.Query["id"].ToString() : null,
                    hasForm = Request.HasFormContentType,
                    formKeys = Request.HasFormContentType ? (await Request.ReadFormAsync()).Keys.ToArray() : Array.Empty<string>()
                };

                return Json(new { success = false, message = "Invalid request", debug = info });
            }

            var service = await _db.Services.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == resolvedId.Value);
            if (service == null)
                return Json(new { success = false, message = "Service not found" });

            service.Status = ServiceStatus.Active;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), "ApproveService", $"Service '{service.Title}' (id={service.Id}) approved");

            return Json(new { success = true, message = "Service approved" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int? id)
        {
            int? resolvedId = id;

            if (resolvedId == null && Request.Query.ContainsKey("id") && int.TryParse(Request.Query["id"], out var qid))
                resolvedId = qid;

            if (resolvedId == null)
            {
                try
                {
                    var contentType = Request.ContentType ?? string.Empty;
                    if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        using var reader = new StreamReader(Request.Body);
                        var body = await reader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            try
                            {
                                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(body, new System.Text.Json.JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                                if (dict != null && dict.TryGetValue("id", out var v)) resolvedId = v;
                            }
                            catch
                            {
                                if (int.TryParse(body.Trim(), out var single)) resolvedId = single;
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            if (resolvedId == null)
            {
                var info = new
                {
                    contentType = Request.ContentType,
                    queryId = Request.Query.ContainsKey("id") ? Request.Query["id"].ToString() : null,
                    hasForm = Request.HasFormContentType,
                    formKeys = Request.HasFormContentType ? (await Request.ReadFormAsync()).Keys.ToArray() : Array.Empty<string>()
                };

                return Json(new { success = false, message = "Invalid request", debug = info });
            }

            var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == resolvedId.Value);
            if (service == null)
                return Json(new { success = false, message = "Service not found" });

            service.Status = ServiceStatus.Removed;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), "DeleteService", $"Service '{service.Title}' (id={service.Id}) deleted");

            return Json(new { success = true, message = "Service removed" });
        }
    }
}
