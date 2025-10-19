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
using TechWebSol.Services.MapManagement;
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
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ApplicationDbContext")
        ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found."),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            // Optimize command timeout
            sqlOptions.CommandTimeout(30);
        });
    
            // Performance optimizations
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // Default to no-tracking for better performance
            options.EnableSensitiveDataLogging(false); // Disabled for performance
            options.EnableDetailedErrors(builder.Environment.IsDevelopment()); // Only in development
});


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
var mvcBuilder = builder.Services.AddControllersWithViews();

// Only enable Razor runtime compilation in development (huge performance overhead in production)
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

mvcBuilder.AddNewtonsoftJson(options =>
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

// Offline Map Services
builder.Services.AddSingleton<IOfflineMapService, OfflineMapService>();

// Map Management Services (for JobsController)
builder.Services.AddSingleton<TechWebSol.Services.MapManagement.JobStore>();
builder.Services.AddHostedService<StartupCleanup>();
builder.Services.AddHttpClient<TileService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("TechWebSol.NET/1.0 (+mailto:you@example.com)");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

// Register TerrainDownloadService with its own HttpClient for elevation/OSM data
builder.Services.AddHttpClient<TerrainDownloadService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120); // Longer timeout for terrain data
    client.DefaultRequestHeaders.UserAgent.ParseAdd("TechWebSol.NET/1.0 (+mailto:you@example.com)");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});


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
builder.Services.AddScoped<ISuspectedTokenMatchingService, SuspectedTokenMatchingService>();
builder.Services.AddScoped<IComprehensiveCombatSimulationService, ComprehensiveCombatSimulationService>();

// Weapon-level combat simulation services
builder.Services.AddScoped<IWeaponEffectivenessService, WeaponEffectivenessService>();
builder.Services.AddScoped<IUnitCombatCalculatorService, UnitCombatCalculatorService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// MVC filters and binders
builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(DynamicAuthorizationFilter));
    options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
});

// Response Caching for Map Tiles and Metadata
builder.Services.AddResponseCaching();

// Memory Cache for frequently accessed data
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size
    options.CompactionPercentage = 0.25; // Compact 25% when limit is reached
});

// Response Compression for better network performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/javascript", "text/css", "text/html" });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
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

// Response Compression (must be before static files)
app.UseResponseCompression();

app.UseHttpsRedirection();

// Static Files with caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 7 days
        const int durationInSeconds = 60 * 60 * 24 * 7;
        ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});

app.UseRouting();

// Response Caching Middleware (must be before Authentication)
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseRequestLocalization();

// Endpoint Configuration
app.MapControllers(); // Enable attribute routing

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();