
namespace TechWebSol.ViewComponents
{
    using Microsoft.AspNetCore.Mvc;

    public class FooterViewComponent : ViewComponent
    {
        public FooterViewComponent()
        {
        }
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}