using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniLocalizer;
using RazorPage.Services;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using NUglify;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace UniLocalizer.Pages
{
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Client)]
    public class ScriptModel : PageModel
    {
        public IRazorToStringRenderer Renderer;
        public UniLocalizerFactory LocalizerFactory;
        public List<ResourceFile> RequestedResourceFiles;
        public string JavascriptNamespace;
        public bool IsJavascriptNamespaceGlobal;
        public readonly IMemoryCache Cache;


        public ScriptModel(IRazorToStringRenderer renderer, IStringLocalizerFactory localizerFactory, IMemoryCache cache)
        {
            this.Renderer = renderer;
            this.LocalizerFactory = (UniLocalizerFactory)localizerFactory;
            this.JavascriptNamespace = this.LocalizerFactory.Provider.options.Value.JavascriptNamespace;
            this.IsJavascriptNamespaceGlobal = this.JavascriptNamespace.StartsWith("window");
            this.Cache = cache;
        }

        public ActionResult OnGet(List<int> f, bool debug = false)
        {
            var scriptResult = this.ScriptResult(f, debug);
            return Content(scriptResult, "application/javascript");
        }

        public string ScriptResult(List<int> f, bool debug = false)
        {
            var cacheItemId = $"Localizer_Script_{ string.Join("_", f) }_";
            // try recive script content from cache
            var scriptResult = this.Cache.Get<string>(cacheItemId);

            if (debug == false && scriptResult != null)
            {
                return scriptResult;
            }

            // filter list of file to those specified by f paramter value (each int value corresponds with file item).
            this.RequestedResourceFiles = this.LocalizerFactory.Provider
                .LoadedFiles
                .Where(k => f.Contains(k.Value.Index))
                .Select(e => e.Value)
                .ToList();

            // render script page vie into string.
            var viewResult = this.Renderer.RenderToString("~/Areas/Localizer/Pages/Script.cshtml", this);

            // returned html should contain script tags (that allow intelisense to work), we have to remove them before serving output.
            var regex = string.Format("<{0}[^>]*>(.*?)</{0}>", "script");
            var res = Regex.Match(viewResult, regex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Singleline);
            if (res.Success && res.Groups.Count > 1)
            {
                // this variable holds "cleaned" javascript code.
                var unwrappedScript = res.Groups[1].Value;

                if (debug == false)
                {
                    var minifiedScript = Uglify.Js(unwrappedScript);

                    if (minifiedScript.HasErrors) throw new Exception($"An error occured while js minification: { minifiedScript.Errors }");

                    scriptResult = minifiedScript.Code;

                }
                else
                {
                    scriptResult = unwrappedScript;
                }
            }
            else
                throw new Exception("Dynamic content produced by viewResult is expected to be wrapped in <script> tag");

            var opts = new MemoryCacheEntryOptions();
            opts.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365);


            // add file dependency to cached data
            this.RequestedResourceFiles.ForEach(file =>
               {
                   var cacheFileItemKey = "Localizer_File_" + file.Index;
                   CancellationTokenSource tokenSource = this.Cache.Get<CancellationTokenSource>(cacheFileItemKey);
                   if (tokenSource == null)
                   {
                       tokenSource = new CancellationTokenSource();
                       this.Cache.Set(cacheFileItemKey, tokenSource);
                       opts.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
                   }
               });

            

            this.Cache.Set(cacheItemId, scriptResult, opts);
            this.Cache.Set(cacheItemId + "Hash", scriptResult.GetHashString(), opts);

            return scriptResult;
        }

        public ActionResult OnGetResult()
        {
            return Page();
        }
    }
}