namespace TechWebSol.ViewComponents
{
    using TechWebSol.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using TechWebSol.ViewModels;
    using TechWebSol.Services;

    public class HeaderViewComponent : ViewComponent
    {
        private readonly ApplicationUserVM applicatonUser;

        public HeaderViewComponent(IUserSessionService IUserSessionService)
        {
            applicatonUser = IUserSessionService.GetCurrentUser();
        }
       

        public IViewComponentResult Invoke()
        {
            ViewBag.CurrentUser = applicatonUser;
            return View();
        }
    }
}