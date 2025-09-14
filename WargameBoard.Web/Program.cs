using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WargameBoard.Core.Data;
using WargameBoard.Core.Services;
using WargameBoard.Web.Data;
using WargameBoard.Web.Hubs;
using WargameBoard.Web.Mapping;
using WargameBoard.Web.Services;
using WargameBoard.Web.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv => 
    {
        fv.RegisterValidatorsFromAssemblyContaining<ScenarioEditVmValidator>();
    });

// Enable runtime compilation for Razor
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


    builder.Services.AddDbContext<WargameDbContext>(options =>
        options.UseSqlServer(connectionString));


// Register services
builder.Services.AddScoped<ITokenAssignmentService, TokenAssignmentService>();
builder.Services.AddScoped<IPlacementService, PlacementService>();
builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
builder.Services.AddScoped<ISelectListService, SelectListService>();
builder.Services.AddScoped<GameSessionService>();
builder.Services.AddScoped<IRealTimeGameService, RealTimeGameService>();

// Add SignalR
builder.Services.AddSignalR();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

await DbSeeder.RunAsync(app.Services);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configure areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<GameHub>("/gameHub");
app.MapHub<PlacementsHub>("/hubs/placements");
app.MapHub<RealTimeGameHub>("/hubs/realtime");

app.Run();
