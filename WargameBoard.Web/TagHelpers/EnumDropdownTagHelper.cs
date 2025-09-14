using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.Reflection;

namespace WargameBoard.Web.TagHelpers
{
    [HtmlTargetElement("enum-dropdown", Attributes = "asp-for")]
    public class EnumDropdownTagHelper : TagHelper
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
        public string EmptyText { get; set; } = "-- Select --";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.TagMode = TagMode.StartTagAndEndTag;

            // Add CSS classes
            var classes = "form-select";
            if (!string.IsNullOrEmpty(CssClass))
            {
                classes += " " + CssClass;
            }
            output.Attributes.SetAttribute("class", classes);

            // Add name and id attributes
            output.Attributes.SetAttribute("name", For.Name);
            output.Attributes.SetAttribute("id", For.Name);

            // Get the enum type
            var enumType = For.ModelExplorer.ModelType;
            if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                enumType = Nullable.GetUnderlyingType(enumType);
            }

            if (enumType == null || !enumType.IsEnum)
            {
                throw new InvalidOperationException($"The property {For.Name} is not an enum type.");
            }

            // Build options
            var options = new List<string>();

            if (IncludeEmpty)
            {
                options.Add($"<option value=\"\">{EmptyText}</option>");
            }

            var enumValues = Enum.GetValues(enumType);
            var currentValue = For.Model?.ToString();

            foreach (var enumValue in enumValues)
            {
                var value = enumValue.ToString();
                var displayName = GetDisplayName(enumType, value);
                var selected = value == currentValue ? " selected" : "";
                
                options.Add($"<option value=\"{value}\"{selected}>{displayName}</option>");
            }

            output.Content.SetHtmlContent(string.Join("", options));
        }

        private string GetDisplayName(Type enumType, string value)
        {
            var field = enumType.GetField(value!);
            if (field == null) return value!;

            var displayAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
            if (displayAttribute != null)
            {
                return displayAttribute.DisplayName;
            }

            // Convert PascalCase to Title Case
            return System.Text.RegularExpressions.Regex.Replace(value!, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
