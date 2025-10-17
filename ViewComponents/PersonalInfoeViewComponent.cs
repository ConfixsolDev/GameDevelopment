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

    public class PersonalInfoeViewComponent : ViewComponent
    {
        private readonly ApplicationUserVM applicatonUser;
        private readonly ApplicationDbContext _dbContext;

        public PersonalInfoeViewComponent(IUserSessionService IUserSessionService, ApplicationDbContext dbContext)
        {
            applicatonUser = IUserSessionService.GetCurrentUser();
            _dbContext = dbContext;
        }
       

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}