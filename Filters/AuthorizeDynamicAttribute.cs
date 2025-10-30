using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using TechWebSol.Services;

namespace TechWebSol.Filters
{
    //public class AuthorizeDynamicAttribute : Attribute
    //{
    //    // No properties needed, just used as a marker
    //}

    //public class DynamicAuthorizationFilter : IAsyncActionFilter
    //{
    //    private readonly IUserSessionService _userSessionService;

    //    public DynamicAuthorizationFilter(IUserSessionService userSessionService)
    //    {
    //        _userSessionService = userSessionService;
    //    }

    //    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    //    {
    //        if (!IsProtectedAction(context))
    //        {
    //            await next();
    //            return;
    //        }

    //        var currentUserRoles = _userSessionService.GetCurrentRole();
    //        if (currentUserRoles == null)
    //        {
    //            context.Result = new RedirectResult("~/Account/Login");
    //            return;
    //        }

    //        var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

    //        var areaName = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue;
    //        var controllerName = controllerActionDescriptor.ControllerName;
    //        var actionName = controllerActionDescriptor.ActionName;

    //        var isAuthorized = currentUserRoles.Any(area => area.AreaName == areaName &&
    //                                                         area.Controller.Any(controller => controller.Id == controllerName &&
    //                                                                                            controller.Actions.Any(action => action.Name == actionName)));

    //        if (!isAuthorized)
    //        {
    //            context.Result = new RedirectResult("~/Account/AccessDenied");
    //            return;
    //        }

    //        // Proceed with the action
    //        await next();
    //    }

    //    private static bool IsProtectedAction(ActionContext context)
    //    {
    //        var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
    //        if (controllerActionDescriptor != null)
    //        {
    //            bool isAuthorize = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
    //                .Any(a => a is AuthorizeAttribute) || controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true)
    //                .Any(a => a is AuthorizeAttribute);

    //            if (isAuthorize)
    //            {
    //                return false; // Skip custom authorization checks if Authorize is present
    //            }
    //        }

    //        // Check for AuthorizeDynamic attribute
    //        var controllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;
    //        var actionMethodInfo = controllerActionDescriptor.MethodInfo;

    //        return controllerTypeInfo.GetCustomAttribute<AuthorizeDynamicAttribute>() != null ||
    //               actionMethodInfo.GetCustomAttribute<AuthorizeDynamicAttribute>() != null;
    //    }
    //}

}
