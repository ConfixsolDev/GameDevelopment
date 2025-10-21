using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TechWebSol.Services;
using TechWebSol.ViewModels;

namespace TechWebSol.TagHelpers
{
    [HtmlTargetElement("secure-content")]
    public class SecureContentTagHelper : TagHelper
    {
        private readonly IUserSessionService _userSessionService;

        public SecureContentTagHelper(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        }

        [HtmlAttributeName("asp-area")]
        public string Area { get; set; }

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null; // Removes the original tag from output.
            var userRoles = _userSessionService.GetCurrentRole();

            // Early exit if no roles or action specified.
            if (userRoles == null || string.IsNullOrEmpty(Action))
            {
                output.SuppressOutput();
                return;
            }

            // Normalize Area to null if empty to simplify comparisons.
            var effectiveArea = string.IsNullOrEmpty(Area) ? null : Area;

            bool hasAccess = userRoles.Any(role =>
                (role.AreaName == effectiveArea || effectiveArea == null) &&
                role.Controller.Any(controller => controller.Id == Controller &&
                controller.Actions.Any(action => action.Name == Action)));

            if (!hasAccess)
            {
                output.SuppressOutput();
            }
        }
    }

    [HtmlTargetElement("secure-Approval")]
    public class SecureApprovalTagHelper : TagHelper
    {
        private readonly IUserSessionService _userSessionService;
        private readonly ApplicationUserVM applicatonUser;
        public SecureApprovalTagHelper(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            applicatonUser = userSessionService.GetCurrentUser();
        }

        [HtmlAttributeName("asp-area")]
        public string Area { get; set; }

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        [HtmlAttributeName("asp-personnel")]
        public string Personnel { get; set; }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null; // Removes the original tag from output.

            if (Personnel == applicatonUser.ApplicationUserId.ToString())
            {
                output.SuppressOutput();
                return;
            }

            var userRoles = _userSessionService.GetCurrentRole();
            // Early exit if no roles or action specified.
            if (userRoles == null || string.IsNullOrEmpty(Action))
            {
                output.SuppressOutput();
                return;
            }

            // Normalize Area to null if empty to simplify comparisons.
            var effectiveArea = string.IsNullOrEmpty(Area) ? null : Area;

            bool hasAccess = userRoles.Any(role =>
                (role.AreaName == effectiveArea || effectiveArea == null) &&
                role.Controller.Any(controller => controller.Id == Controller &&
                controller.Actions.Any(action => action.Name == Action)));

            if (!hasAccess)
            {
                output.SuppressOutput();
            }
        }
    }
}
