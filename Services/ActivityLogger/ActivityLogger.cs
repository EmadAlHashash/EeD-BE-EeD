using System;
using System.Threading.Tasks;
using EeD_BE_EeD.Data;
using EeD_BE_EeD.Models;
using Microsoft.EntityFrameworkCore;

namespace EeD_BE_EeD.Services.ActivityLogger
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly ApplicationDbContext _db;

        public ActivityLogger(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string? userId, string action, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("action is required", nameof(action));

            var log = new ActivityLog
            {
                UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                Action = action.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
