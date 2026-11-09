using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Webshop.Library.Data;
using Webshop.Library.Models;
using Webshop.Library.Services;
using Webshop.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Beskytter kundesider og administratorsider med de relevante adgangskrav.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Orders");
    options.Conventions.AuthorizeFolder("/Checkout");
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizePage("/Account/ChangePassword");
});
builder.Services.AddAuthorization(options => options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")));
// Login gemmes i en sikker cookie, som JavaScript ikke kan læse.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = "SecureWebshop.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
// Sessionen bruges kun til den enkelte brugers indkøbskurv.
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "SecureWebshop.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

// Bruger den konfigurerede forbindelse eller den lokale udviklingsdatabase.
var connectionString = builder.Configuration.GetConnectionString("Webshop")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=SecureCodingWebshop;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddSingleton(new SqlConnectionFactory(connectionString));
builder.Services.AddScoped<StoreRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<CartSession>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
// Tilføjer grundlæggende sikkerheds-headere til alle svar.
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers.ContentSecurityPolicy = "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self'; img-src 'self' data:";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
