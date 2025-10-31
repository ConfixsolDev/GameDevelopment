using Microsoft.AspNetCore.Mvc;

namespace TechWebSol.Controllers.MapManagement
{
    public class MapManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Downloads()
        {
            return View();
        }

        public IActionResult Viewer(string file)
        {
            ViewBag.FileName = file;
            return View();
        }

        public IActionResult MBTiles()
        {
            return View();
        }

        public IActionResult TacticalViewer(string file)
        {
            ViewBag.FileName = file;
            return View();
        }

        public IActionResult LeafletBounded(string file)
        {
            ViewBag.FileName = file;
            return View();
        }
    }
}


