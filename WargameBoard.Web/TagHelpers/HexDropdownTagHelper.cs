using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WargameBoard.Web.TagHelpers
{
    [HtmlTargetElement("hex-dropdown", Attributes = "asp-for")]
    public class HexDropdownTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; } = null!;

        [HtmlAttributeName("asp-class")]
        public string? CssClass { get; set; }

        [HtmlAttributeName("asp-placeholder")]
        public string? Placeholder { get; set; }

        [HtmlAttributeName("asp-include-empty")]
        public bool IncludeEmpty { get; set; } = true;

        [HtmlAttributeName("asp-empty-text")]
        public string EmptyText { get; set; } = "-- Select Hex --";

        [HtmlAttributeName("asp-max-q")]
        public int MaxQ { get; set; } = 10;

        [HtmlAttributeName("asp-max-r")]
        public int MaxR { get; set; } = 10;

        [HtmlAttributeName("asp-min-q")]
        public int MinQ { get; set; } = -10;

        [HtmlAttributeName("asp-min-r")]
        public int MinR { get; set; } = -10;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.TagMode = TagMode.StartTagAndEndTag;

            // Add CSS classes
            var classes = "form-select hex-dropdown";
            if (!string.IsNullOrEmpty(CssClass))
            {
                classes += " " + CssClass;
            }
            output.Attributes.SetAttribute("class", classes);

            // Add name and id attributes
            output.Attributes.SetAttribute("name", For.Name);
            output.Attributes.SetAttribute("id", For.Name);

            // Build options
            var options = new List<string>();

            if (IncludeEmpty)
            {
                options.Add($"<option value=\"\">{EmptyText}</option>");
            }

            var currentValue = For.Model?.ToString();

            // Generate hex coordinates
            for (int q = MinQ; q <= MaxQ; q++)
            {
                for (int r = MinR; r <= MaxR; r++)
                {
                    var hexValue = $"{q},{r}";
                    var displayText = $"Q{q}, R{r}";
                    var selected = hexValue == currentValue ? " selected" : "";
                    
                    options.Add($"<option value=\"{hexValue}\"{selected}>{displayText}</option>");
                }
            }

            output.Content.SetHtmlContent(string.Join("", options));
        }
    }
}
