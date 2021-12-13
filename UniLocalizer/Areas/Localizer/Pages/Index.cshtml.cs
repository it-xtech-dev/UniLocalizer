using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using UniLocalizer;
using UniLocalizer.Extensions;

namespace UniLocalizer.Pages
{
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class IndexModel : PageModel
    {
        public UniLocalizerFactory LocalizerFactory;

        public IndexModel(IStringLocalizerFactory localizerFactory)
        {
            this.LocalizerFactory = (UniLocalizerFactory)localizerFactory;
        }

        /// <summary>
        /// Sets current culture for application.
        /// </summary>
        /// <param name="culture">The culture to be set.</param>
        /// <param name="returnUrl">The return parameter to be set.</param>
        /// <returns></returns>
        public LocalRedirectResult OnGetSetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture)
                ),
                new CookieOptions { 
                    Expires = DateTimeOffset.UtcNow.AddDays(30), 
                    IsEssential = true 
                }
            );

            return LocalRedirect(returnUrl);
        }

        /// <summary>
        /// Forces localizer to save translations from current state (application) to storage (json / db).
        /// </summary>
        /// <param name="cultureName"></param>
        /// <returns></returns>
        public IActionResult OnPostWriteLanguageResources(string cultureName = null)
        {
            this.LocalizerFactory.Provider.WriteResources(cultureName);
            return new JsonResult(new { status = "ok" });
        }

        /// <summary>
        /// Forces localizer to reload all its resources from source.
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostReloadLanguageResources()
        {
            this.LocalizerFactory.Provider.Reload();
            return new JsonResult(new { status = "ok" });
        }

        /// <summary>
        /// Updates translation resource item.
        /// </summary>
        /// <param name="generalKey">The key that identifies culture, resource location and resource / token name eg: en-US:.views.home.index:header_welcome</param>
        /// <param name="value">The value that will be assigned</param>
        /// <returns></returns>
        public IActionResult OnPostUpdateLocalizerItem(string generalKey, string value)
        {
            var item = this.LocalizerFactory.Provider.ItemList.GetValue(generalKey.GetHashString());
            if (item != null)
            {
                item.Value = value;
                return new JsonResult(new { status = "ok" });
            }

            return new JsonResult(new { status = "error", reason = "key not found" });
        }

        /// <summary>
        /// Gets text for specified translation key.
        /// </summary>
        /// <param name="generalKey">The key that identifies culture, resource location and resource / token name eg: en-US:.views.home.index:header_welcome</param>
        /// <returns></returns>
        public IActionResult OnPostGetText(string generalKey)
        {
            var text = this.LocalizerFactory.Provider.Get(generalKey);
            return new JsonResult(new
            {
                status = "ok",
                data = new
                {
                    value = text
                }
            });
        }

        /// <summary>
        /// Filters handler execution according to localizer settings.
        /// </summary>
        /// <param name="context">The context</param>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (context.HandlerMethod.Name != "SetLanguage")
            {
                if (!LocalizerFactory.Options.IsTranslatorEnabled)
                {
                    context.Result = new NotFoundResult();
                }
                else if (
                      LocalizerFactory.Options.TraslatorUserRole != null
                      && !User.IsInRole(LocalizerFactory.Options.TraslatorUserRole)
                      )
                {
                    context.Result = new NotFoundResult();
                }
            }
        }
    }
}