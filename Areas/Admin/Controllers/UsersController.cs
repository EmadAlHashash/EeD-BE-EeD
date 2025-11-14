using System.Diagnostics;
using EeD_BE_EeD.Data;
using EeD_BE_EeD.Models;
using EeD_BE_EeD.Services.ActivityLogger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EeD_BE_EeD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _db;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env,
            IActivityLogger activityLogger,
            ILogger<UsersController> logger,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
            _activityLogger = activityLogger;
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(
            [FromForm] EeD_BE_EeD.Areas.Admin.ViewModels.AddAdminViewModel model,
            IFormFile AvatarFile)
        {
            _logger.LogInformation("STEP 1: Creating admin…");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            // Ensure email unique
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                return Json(new { success = false, message = "Email already exists" });
            }

            // Build user entity
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Status = model.Status ?? "Active"
            };

            // Save avatar if provided
            try
            {
                _logger.LogInformation("STEP 2: Saving avatar…");
                var file = AvatarFile ?? model.AvatarFile;
                if (file != null && file.Length > 0)
                {
                    var avatarUrl = await SaveAvatarAsync(file);
                    user.ProfilePictureUrl = avatarUrl;
                    Debug.WriteLine($"[AddAdmin] Avatar uploaded: {avatarUrl}");
                    _logger.LogInformation("Avatar uploaded to {Url}", avatarUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Avatar upload failed");
                return Json(new { success = false, message = "Avatar upload failed" });
            }

            // Create user with hashed password
            _logger.LogInformation("STEP 3: Saving user to DB…");
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = msg });
            }

            Debug.WriteLine($"[AddAdmin] User saved: {user.Id}");
            _logger.LogInformation("User saved with Id {Id}", user.Id);

            // Assign role
            _logger.LogInformation("STEP 4: Assigning role…");
            var role = string.IsNullOrWhiteSpace(model.Role) ? "Admin" : model.Role.Trim();
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var msg = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Failed to assign role: {msg}" });
            }
            Debug.WriteLine($"[AddAdmin] Roles applied: {role}");
            _logger.LogInformation("Roles applied: {Role}", role);

            // Activity log
            _logger.LogInformation("STEP 5: Saving ActivityLog…");
            await _activityLogger.LogAsync(user.Id, "AddAdmin", $"Admin created: {user.Email} with role {role}");
            Debug.WriteLine("[AddAdmin] ActivityLog inserted");
            _logger.LogInformation("ActivityLog inserted");

            DebugUser(user, role);

            return Json(new { success = true, message = "Created", id = user.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(
            [FromForm] EeD_BE_EeD.Areas.Admin.ViewModels.EditUserViewModel model,
            IFormFile AvatarFile)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            // find user
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // email uniqueness
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var byEmail = await _userManager.FindByEmailAsync(model.Email);
                if (byEmail != null && byEmail.Id != user.Id)
                {
                    return Json(new { success = false, message = "Email already exists" });
                }
            }

            // update fields
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            // Keep Status as-is (unless editing explicitly via model)
            user.Status = model.Status ?? user.Status;

            // avatar replace
            try
            {
                var file = AvatarFile ?? model.AvatarFile;
                if (file != null && file.Length > 0)
                {
                    var avatarUrl = await SaveAvatarAsync(file);
                    user.ProfilePictureUrl = avatarUrl;
                    Debug.WriteLine($"[EditUser] Avatar uploaded: {avatarUrl}");
                    _logger.LogInformation("Avatar uploaded to {Url}", avatarUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Avatar upload failed");
                return Json(new { success = false, message = "Avatar upload failed" });
            }

            // persist user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var msg = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                return Json(new { success = false, message = msg });
            }
            Debug.WriteLine($"[EditUser] User updated: {user.Id}");
            _logger.LogInformation("User updated: {Id}", user.Id);

            // update role
            var desiredRole = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role.Trim();
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var msg = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Failed to remove roles: {msg}" });
            }
            if (!await _roleManager.RoleExistsAsync(desiredRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(desiredRole));
            }
            var addRoleResult = await _userManager.AddToRoleAsync(user, desiredRole);
            if (!addRoleResult.Succeeded)
            {
                var msg = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                return Json(new { success = false, message = $"Failed to assign role: {msg}" });
            }
            Debug.WriteLine($"[EditUser] Roles applied: {desiredRole}");
            _logger.LogInformation("Roles applied: {Role}", desiredRole);

            await _activityLogger.LogAsync(user.Id, "EditUser", $"User edited: {user.Email}");
            Debug.WriteLine("[EditUser] ActivityLog inserted");
            _logger.LogInformation("ActivityLog inserted");

            DebugUser(user, desiredRole);

            return Json(new { success = true, message = "Updated" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser([FromForm] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Json(new { success = false, message = "Invalid user id" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            user.Status = "Banned";
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = msg });
            }

            await _activityLogger.LogAsync(user.Id, "BlockUser", $"User blocked: {user.Email}");
            return Json(new { success = true, message = "Blocked" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser([FromForm] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Json(new { success = false, message = "Invalid user id" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            user.Status = "Inactive"; // default to Inactive when unblocked (not logged in yet)
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = msg });
            }

            await _activityLogger.LogAsync(user.Id, "UnblockUser", $"User unblocked: {user.Email}");
            return Json(new { success = true, message = "Unblocked" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromForm] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Json(new { success = false, message = "Invalid user id" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = msg });
            }

            await _activityLogger.LogAsync(userId, "DeleteUser", $"User deleted: {user.Email}");
            return Json(new { success = true, message = "Deleted" });
        }

        private async Task<string> SaveAvatarAsync(IFormFile file)
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }
            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsRoot, fileName);
            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            var publicUrl = $"/uploads/avatars/{fileName}";
            return publicUrl;
        }

        private void DebugUser(ApplicationUser user, string? role = null)
        {
            var msg = $"DebugUser => ID: {user.Id}, Email: {user.Email}, Role: {role ?? string.Join(',', _userManager.GetRolesAsync(user).GetAwaiter().GetResult())}, Status: {user.Status}, Avatar: {user.ProfilePictureUrl}";
            Debug.WriteLine(msg);
            _logger.LogInformation(msg);
        }
    }
}
