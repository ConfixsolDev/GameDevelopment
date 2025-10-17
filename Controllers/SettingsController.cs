using TechWebSol.Filters;
using TechWebSol.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    [DisplayName("Settings: User Roles and Application Drop Down List (Assigned to Management)")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [DisplayName("Settings: List Permissions (Management)")]
        public IActionResult Index()
        {
            return View();
        }
    }
}