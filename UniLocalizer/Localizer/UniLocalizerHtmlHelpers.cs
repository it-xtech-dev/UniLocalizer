using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using UniLocalizer.Pages;
using RazorPage.Services;

namespace UniLocalizer
{
    /// <summary>
    /// Provides UniLocalizer HtmlHelpers
    /// TODO: Do not depend on controller / action architected. Move to "module" resouce serving solution for javascript.
    /// </summary>
    public static class UniLocalizerHtmlHelpers
    {
        /// <summary>
        /// Renders script tags containg resouces for given file location
        /// </summary>
        /// <param name="helper">The helper instance.</param>
        /// <param name="fileResourceKeys">Resource location key (path separated with dots) path1.path2.filename. Use null reference current view / page resource.</param>
        /// <returns>Html string containing script tag.</returns>
        public static HtmlString JsLocalizer(this IHtmlHelper helper, params string[] fileResourceKeys)
        {
            //helper.ViewContext.ExecutingFilePath;

            var context = helper.ViewContext.HttpContext;
            IMemoryCache cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            UniLocalizerFactory localizerFactory = (UniLocalizerFactory)context.RequestServices.GetRequiredService<IStringLocalizerFactory>();
            var urlHelperFactory = (IUrlHelperFactory)helper.ViewContext.HttpContext.RequestServices.GetService(typeof(IUrlHelperFactory));
            var urlHelper = urlHelperFactory.GetUrlHelper(helper.ViewContext);

            var culture = CultureInfo.CurrentCulture;
            
            var filesQuery = new List<int>();

            if (fileResourceKeys == null)
            {
                fileResourceKeys = new string[1];
                fileResourceKeys[0] = ResolveKeyFromCurrentView();
            }

            fileResourceKeys.ToList().ForEach(key => {
                if (key == null)
                {
                    key = ResolveKeyFromCurrentView();
                }

                var resourceFileKey = culture.Name + ":." + key + ":";
                // load from cache?
                
                if (!localizerFactory.Provider.LoadedFiles.TryGetValue(resourceFileKey.GetHashString(), out var file))
                {
                    throw new Exception($"Unrecognized resource '{ resourceFileKey }'. Please add file to storage first.");
                }
                filesQuery.Add(file.Index);
            });

            if (!filesQuery.Any()) throw new Exception("No resource key specified. Please specifiy at least one resource key.");

            var fileList = filesQuery.OrderBy(f => f).ToList();
            var cacheItemVersionKey = $"Localizer_Script_{ string.Join("_", fileList) }_Hash";
            var cacheItemVersion = cache.Get<string>(cacheItemVersionKey);
            if (cacheItemVersion == null)
            {
                IRazorToStringRenderer renderer = context.RequestServices.GetRequiredService<IRazorToStringRenderer>();
                var model = new ScriptModel(renderer, localizerFactory, cache);

                model.ScriptResult(fileList, false);

                cacheItemVersion = cache.Get<string>(cacheItemVersionKey);
            }

            var scriptResourceLink = urlHelper.Page("/Script", new { area = "Localizer", f = fileList, v = cacheItemVersion/*, debug = true */});

            // todo: add file checksum
            return new HtmlString(String.Format($"<script src='{scriptResourceLink}'></script>"));

            string ResolveKeyFromCurrentView()
            {
                var pathKey = helper.ViewContext.ExecutingFilePath.Replace(@".cshtml", "").Replace(@"/", ".").ToLower();
                var key = pathKey.Substring(1);

                return key;
            }
        }

        /// <summary>
        /// Converts string to javascript string, with " escaped with \" and ' escaped with \'.
        /// </summary>
        /// <param name="text">The text instance</param>
        /// <returns></returns>
        public static HtmlString ToJsString(this LocalizedHtmlString text)
        {
            var result = text.Value.ToJsString();
            return result;
        }

        /// <summary>
        /// Converts string to javascript string, with " escaped with \" and ' escaped with \'.
        /// </summary>
        /// <param name="text">The text instance</param>
        /// <returns></returns>
        public static HtmlString ToJsString(this string text)
        {
            var result = text.Replace("\"", "\\\"").Replace("'", "\\'");
            return new HtmlString(result);
        }

        /// <summary>
        /// Renders MVC view to html string.
        /// TODO: Move to more general helper namespace.
        /// </summary>
        /// <param name="result">The view result object instance.</param>
        /// <param name="httpContext">The http context for rendering the view.</param>
        /// <returns></returns>
        public static string ToHtml(this ViewResult result, HttpContext httpContext)
        {
            var feature = httpContext.Features.Get<IRoutingFeature>();
            var routeData = feature.RouteData;
            var viewName = result.ViewName ?? routeData.Values["action"] as string;
            var actionContext = new ActionContext(httpContext, routeData, new ControllerActionDescriptor());
            var options = httpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();
            var htmlHelperOptions = options.Value.HtmlHelperOptions;
            var viewEngineResult = result.ViewEngine?.FindView(actionContext, viewName, true) ?? options.Value.ViewEngines.Select(x => x.FindView(actionContext, viewName, true)).FirstOrDefault(x => x != null);
            var view = viewEngineResult.View;
            var builder = new StringBuilder();

            using (var output = new StringWriter(builder))
            {
                var viewContext = new ViewContext(actionContext, view, result.ViewData, result.TempData, output, htmlHelperOptions);

                view
                    .RenderAsync(viewContext)
                    .GetAwaiter()
                    .GetResult();
            }

            return builder.ToString();
        }
    }
}
