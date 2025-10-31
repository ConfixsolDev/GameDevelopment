using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TechWebSol.Services
{
    public interface IReportRenderService
    {
        Task<string> RenderViewToStringAsync(ControllerContext controllerContext, string viewPath, object model);
    }

    public class ReportRenderService : IReportRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ReportRenderService(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(ControllerContext controllerContext, string viewPath, object model)
        {
            var actionContext = controllerContext ?? new ControllerContext(new ActionContext(
                controllerContext?.HttpContext,
                controllerContext?.RouteData,
                new ActionDescriptor()));

            using var sw = new StringWriter();

            var viewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: false);
            if (!viewResult.Success)
            {
                // Try get by absolute path
                viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);
            }

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"Could not find view '{viewPath}'.");
            }

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}


