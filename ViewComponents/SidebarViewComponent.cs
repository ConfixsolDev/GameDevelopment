
namespace TechWebSol.ViewComponents
{
    using Microsoft.AspNetCore.Mvc;

    public class SidebarViewComponent : ViewComponent
    {
        public SidebarViewComponent()
        {
        }

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}