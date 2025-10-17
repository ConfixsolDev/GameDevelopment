using Microsoft.AspNetCore.Mvc;
using TechWebSol.Filters;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HandleErrorCode(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("Error404");
        }

        return View("Error");
    }

    [Route("Error")]
    public IActionResult Error()
    {
        return View("Error");
    }
}
