using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TechWebSol.Services;

namespace TechWebSol.Filters
{
    public class AuthorizeDynamicAttribute : TypeFilterAttribute
    {
        public AuthorizeDynamicAttribute() : base(typeof(AuthorizeDynamicFilter))
        {
        }
    }

    public class AuthorizeDynamicFilter : IAuthorizationFilter
    {
        private readonly IUserSessionService _userSessionService;

        public AuthorizeDynamicFilter(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization for authentication-related actions to prevent redirect loops
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();
            var areaName = context.RouteData.Values["area"]?.ToString();

            // Allow access to authentication-related actions
            if (IsAuthenticationAction(controllerName, actionName, areaName))
            {
                return;
            }

            var user = _userSessionService.GetCurrentUser();
            if (user == null)
            {
                // Redirect to the correct login page in the Identity area
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Basic authorization - you can enhance this with role-based checks
            // For now, just ensure user is authenticated
        }

        private bool IsAuthenticationAction(string controllerName, string actionName, string areaName)
        {
            // Allow access to authentication-related actions
            if (string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "AccountAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Allow access to Error pages
            if (string.Equals(controllerName, "Error", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
