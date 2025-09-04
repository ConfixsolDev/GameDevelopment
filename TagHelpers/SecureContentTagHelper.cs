using Microsoft.AspNetCore.Razor.TagHelpers;
using TechWebSol.Services;

namespace TechWebSol.TagHelpers
{
    [HtmlTargetElement("secure-content")]
    public class SecureContentTagHelper : TagHelper
    {
        private readonly IUserSessionService _userSessionService;

        public SecureContentTagHelper(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        [HtmlAttributeName("asp-area")]
        public string Area { get; set; } = string.Empty;

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; } = string.Empty;

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _userSessionService.GetCurrentUser();
            
            // For now, always show content if user is authenticated
            // You can enhance this with specific permission checks
            if (user == null)
            {
                output.SuppressOutput();
                return;
            }

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}
