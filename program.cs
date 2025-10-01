using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.Services.TokenManagement;

var builder = WebApplication.CreateBuilder(args);
 
// Configure Kestrel to listen on port 5030
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5030);
});

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ApplicationDbContext")
        ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found."),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));


// Configure Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "techwebsolCookie";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Reduced from 300 minutes
    options.SlidingExpiration = true; // Reset expiration on activity
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Authentication cookie expires when browser closes (non-persistent)
    options.Cookie.Expiration = null; // This makes it a session cookie
});

// MVC, JSON, and Custom Configurations
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

// Session, Cookie, and HttpContext Accessor
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "TechWebSolSession";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

// Configure FormOptions for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-US") };
    supportedCultures[0].DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";  // Custom date format

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Custom Services - Local implementations
builder.Services.AddSingleton<IMvcControllerDiscovery, MvcControllerDiscovery>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Token Management Services
builder.Services.AddScoped<ITokenPlacementService, TokenPlacementService>();
builder.Services.AddScoped<ITokenAreaCoverageService, TokenAreaCoverageService>();

// Pattern Matching Services
builder.Services.AddScoped<IPatternMatchingService, PatternMatchingService>();
builder.Services.AddScoped<PatternAnalysisEngine>();

// Simplified Pattern Matching Services

// Unified Token Identification DAL
builder.Services.AddScoped<TokenIdentificationDAL>();

// Enhanced Wargame Services
builder.Services.AddScoped<IMovementService, MovementService>();
builder.Services.AddScoped<CombatService>();
builder.Services.AddScoped<SupplyService>();
builder.Services.AddScoped<IDetectionService, DetectionService>();
builder.Services.AddScoped<IAttackPreviewService, AttackPreviewService>();
builder.Services.AddScoped<IOrderPersistenceService, OrderPersistenceService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// MVC filters and binders
builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(DynamicAuthorizationFilter));
    options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
});

var app = builder.Build();

// Initialize database with default data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await initializer.InitializeAsync();

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enable forwarding of headers for proxy scenarios
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseRequestLocalization();

// Endpoint Configuration
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();