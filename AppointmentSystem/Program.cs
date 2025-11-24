using AppointmentSystem.Authorization;
using AppointmentSystem.Data;
using AppointmentSystem.Services.Concrete;
using AppointmentSystem.Services.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    // ✅ GLOBAL PERMISSION FILTER - Bütün action-lara tətbiq olunur
    options.Filters.Add<GlobalPermissionAuthorizationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "az", "en", "ru" };
    options.SetDefaultCulture("az")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("AppointmentSystem")));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/SelectCompany"; // ✅ İlk səhifə Company seçimi
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// ✅ Authorization policy yoxdur - Global filter istifadə edilir
builder.Services.AddAuthorization();

// FluentValidation (yeni yol)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Services (Scrutor kitabxanası ilə avtomatik qeydiyyat)
builder.Services.Scan(scan => scan
    .FromAssembliesOf(typeof(MeetingService))
    .AddClasses()
    .AsMatchingInterface()
    .WithScopedLifetime());
builder.Services.AddScoped<PermissionSeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization(); // Localization middleware

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


// ✅ Area Routing (ÖNCƏLİKLİ!)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// ✅ Default Routing (Company seçimi ilə başlasın)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SelectCompany}/{id?}");


#region Database Initialization & Permission Seed

// ✅ Database initialization və Permission seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Database migrations
        //await context.Database.MigrateAsync();

        // Seed data
        await SeedData.InitializeAsync(context);

        // ✅ Permission seed - Route scan
        var permissionSeedService = services.GetRequiredService<PermissionSeedService>();
        await permissionSeedService.SeedPermissionsAsync();

        Console.WriteLine("✅ Database və permissions uğurla hazırlandı");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Database initialization zamanı xəta");
    }
}

#endregion

app.Run();
