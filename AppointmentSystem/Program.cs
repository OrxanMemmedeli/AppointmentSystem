using AppointmentSystem.Data;
using AppointmentSystem.Services.Abstract;
using AppointmentSystem.Services.Conrete;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("TEACHER"));
    options.AddPolicy("ParentOnly", policy => policy.RequireRole("PARENT"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("SUPERADMIN", "MANAGER"));
});

// FluentValidation (yeni yol)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();


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

// Seed Data (İlk işə salınanda)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InitializeAsync(context);
}

app.Run();
