using EeD_BE_EeD.Data;
using EeD_BE_EeD.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;

var builder = WebApplication.CreateBuilder(args);

// ================== DbContext ==================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ================== Identity + Roles ==================
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
// Register Activity Logger service
builder.Services.AddScoped<EeD_BE_EeD.Services.ActivityLogger.IActivityLogger, EeD_BE_EeD.Services.ActivityLogger.ActivityLogger>();

var app = builder.Build();

// ============= Migration + Seeding (once) =============
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
            await SeedAsync(services);
    }
}

static async Task SeedAsync(IServiceProvider services)
{
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");
    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();
    var config = services.GetRequiredService<IConfiguration>();

    string[] roles = { "SuperAdmin", "Admin", "User" };
    foreach (var r in roles)
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));

    // SuperAdmin
    var superEmail = config["SuperAdmin:Email"];
    var superPass = config["SuperAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(superEmail) && !string.IsNullOrWhiteSpace(superPass))
    {
        var super = await userMgr.FindByEmailAsync(superEmail);
        if (super == null)
        {
            super = new ApplicationUser
            {
                UserName = superEmail,
                Email = superEmail,
                EmailConfirmed = true,
                FullName = "Super Admin",
                Country = "Jordan"
            };
            var create = await userMgr.CreateAsync(super, superPass);
            if (create.Succeeded)
                await userMgr.AddToRolesAsync(super, new[] { "SuperAdmin", "Admin", "User" });
        }
    }

    // Admin
    var adminEmail = config["Admin:Email"];
    var adminPass = config["Admin:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPass))
    {
        var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Site Admin",
                Country = "Jordan"
            };
            var create = await userMgr.CreateAsync(admin, adminPass);
            if (create.Succeeded)
                await userMgr.AddToRolesAsync(admin, new[] { "Admin", "User" });
        }
    }

    logger.LogInformation("✅ Seeding finished.");
}

// ================== Pipeline ==================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// لازم ييجي قبل ميدل وير الحماية
app.UseAuthentication();

// ======= Role/Area Guard Middleware =======
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    // 1) اسمح بالمسارات العامة والملفات الثابتة
    bool isStatic = Path.HasExtension(path); // مثل .css/.js/.png...
    if (isStatic
        || path.StartsWithSegments("/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/home", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/identity", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    // 2) لازم تسجيل دخول لأي شيء غير عام
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }

    // 3) حماية مسارات الـ User area
    if (path.StartsWithSegments("/user", StringComparison.OrdinalIgnoreCase))
    {
        // عندك الـAdmin/SuperAdmin معاهم دور User من الـseeding، فبدخلوا عادي
        if (!context.User.IsInRole("User"))
        {
            // مستخدم مش User حاول يدخل صفحات User -> ودّيه للوحة الادمن لو معه صلاحية، وإلا لصفحة المستخدم
            if (context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin"))
                context.Response.Redirect("/Admin/Dashboard/Index");
            else
                context.Response.Redirect("/Identity/Account/AccessDenied");
            return;
        }
    }

    // 4) حماية مسارات الـ Admin area
    if (path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase))
    {
        if (!(context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin")))
        {
            // مستخدم عادي حاول يدخل Admin -> رجّعه لصفحة المستخدم
            context.Response.Redirect("/User/User/UserHome");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

// ================== Routing ==================
// Areas (خلي الافتراضي Dashboard/Index للـAdmin و UserHome/Index لليوزر حسب الكنترولرز عندك)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



// ✅ لتشغيل wwwroot داخل Area User
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(builder.Environment.ContentRootPath, "Areas", "User", "wwwroot")
//    ),
//    RequestPath = "/User"
//});

/// ✅ Scan Areas folder and auto-register static file providers
var areasDir = Path.Combine(builder.Environment.ContentRootPath, "Areas");
if (Directory.Exists(areasDir))
{
    foreach (var areaFolder in Directory.GetDirectories(areasDir))
    {
        var areaName = Path.GetFileName(areaFolder); // ex: User, Admin ...

        var wwwrootPath = Path.Combine(areaFolder, "wwwroot");

        if (Directory.Exists(wwwrootPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(wwwrootPath),
                RequestPath = $"/{areaName}"
            });

            Console.WriteLine($"✅ Static files enabled for Area: {areaName}");
        }
    }
}
// Identity Pages
app.MapRazorPages();

app.Run();
