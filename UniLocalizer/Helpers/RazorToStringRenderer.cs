using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;

namespace RazorPage.Services
{
    //https://stackoverflow.com/questions/46540358/render-a-razor-page-to-string
    public interface IRazorToStringRenderer
    {
        string RenderToString<TModel>(string pageName, TModel model);
    }

    public class RazorToStringRenderer : IRazorToStringRenderer
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IRazorPageActivator _activator;


        public RazorToStringRenderer(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IHttpContextAccessor httpContext,
            IRazorPageActivator activator)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;

            _httpContext = httpContext;
            _activator = activator;

        }

        /// <summary>
        /// Renders reazor page into string,
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="pageName">The page name</param>
        /// <param name="model">The page model</param>
        /// <returns>Page html rendered as string.</returns>
        public string RenderToString<TModel>(string pageName, TModel model)
        {
            var httpContext = _httpContext.HttpContext
                ?? throw new InvalidOperationException("Rendering a Razor page requires an active HTTP request.");

            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData() ?? new RouteData(), new PageActionDescriptor());

            using (var sw = new StringWriter())
            {
                var result = _razorViewEngine.GetPage("", pageName);
                    //FindPage(actionContext, pageName);

                if (result.Page == null)
                {
                    throw new ArgumentNullException($"The page {pageName} cannot be found.");
                }

                var view = new RazorView(_razorViewEngine,
                    _activator,
                    [],
                    result.Page,
                    HtmlEncoder.Default,
                    new DiagnosticListener("ViewRenderService"));


                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        _httpContext.HttpContext,
                        _tempDataProvider
                    ),
                    sw,
                    new HtmlHelperOptions()
                );


                var page = ((Page)result.Page);

                page.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
                {
                    ViewData = viewContext.ViewData

                };

                page.ViewContext = viewContext;


                _activator.Activate(page, viewContext);

                page.ExecuteAsync().GetAwaiter().GetResult();


                return sw.ToString();
            }
        }
    }
}