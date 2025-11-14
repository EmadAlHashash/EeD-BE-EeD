using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EeD_BE_EeD.Middlewares
{
    public class AdminAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // ✅ لو داخل /User area
            if (path.StartsWith("/User", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Identity/Account/Login");
                    return;
                }

                bool isUser = context.User.IsInRole("User");
                bool isAdmin = context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin");

                // ✅ امنع الادمن والسوبر يدخل صفحة اليوزر
                if (!isUser && isAdmin)
                {
                    context.Response.Redirect("/Admin/Dashboard/Index");
                    return;
                }
            }

            await _next(context);
        }
    }

}
