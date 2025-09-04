namespace TechWebSol.ViewComponents
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;

    public class HeaderAuthComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _HTTPContextAccessor;
        public HeaderAuthComponent(IHttpContextAccessor HTTPContextAccessor)
        {
            _HTTPContextAccessor = HTTPContextAccessor;
        }

        public IViewComponentResult Invoke()
        {

            //if (HttpContext.Session.Keys.Count() < 0)
            //{
            //    _HTTPContextAccessor.HttpContext.Response.Redirect("../Identity/Login");
            //}
            return View();
        }
    }
}