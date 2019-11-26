using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniLocalizer;

namespace UniLocalizer.Pages
{
    public class TranslateModel : PageModel
    {
        public CultureInfo FilteredCulture;
        public IList<CultureInfo> SupportedCultures;
        public UniLocalizerFactory LocalizerFactory;
        public Dictionary<string, ResourceItem> AllTranslationItems;
        public Dictionary<string, ResourceItem> FilteredTranslationItems;

        public TranslateModel(IOptions<RequestLocalizationOptions> locOptions, IStringLocalizerFactory localizerFactory)
        {
            this.SupportedCultures = locOptions.Value.SupportedUICultures;
            this.LocalizerFactory = (UniLocalizerFactory)localizerFactory;
            this.AllTranslationItems = this.LocalizerFactory.Provider.ItemList;
        }

        public ActionResult OnGet(string culture)
        {
            if (culture?.ToLower() == "all")
            {
                this.FilteredTranslationItems = this.AllTranslationItems;
            } 
            else
            {
                var filteredCultureInfo = this.SupportedCultures.FirstOrDefault(c => c.Name.ToLower() == culture?.ToLower());
                if (filteredCultureInfo == null && culture != null)
                    throw new System.Exception($"Unsupported culture parameter value: '{culture}'");

                this.FilteredCulture = filteredCultureInfo ?? CultureInfo.CurrentUICulture;

                this.FilteredTranslationItems = AllTranslationItems
                    .Where(i => i.Value.Culture.TwoLetterISOLanguageName == this.FilteredCulture.TwoLetterISOLanguageName)
                    .ToDictionary(l => l.Key, l => l.Value);

            }

            return Page();
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (!LocalizerFactory.Options.IsTranslatorEnabled)
            {
                context.Result = new NotFoundResult();
            } else if (
                    LocalizerFactory.Options.TraslatorUserRole != null
                    && !User.IsInRole(LocalizerFactory.Options.TraslatorUserRole)
                    )
            {
                context.Result = new NotFoundResult();
            }                
        }
    }
}