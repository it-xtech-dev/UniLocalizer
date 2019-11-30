using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UniLocalizer.Providers;

namespace UniLocalizer
{
    /// <summary>
    /// Provides localizer class.
    /// </summary>
    public class UniLocalizer : IStringLocalizer
    {
       
        private IStorageProvider provider;
        private CultureInfo culture;
        private string resourceLocation;


        /// <summary>
        /// Initializes new instance of localizer with given provider and resource file location.
        /// </summary>
        /// <param name="provider">The resource provider.</param>
        /// <param name="resourceLocation">The resource file location key ex. .data.file -> @localeroot/data/file.{culture.name}.json </param>
        public UniLocalizer(IStorageProvider provider, string resourceLocation)
        {
            this.provider = provider;
            this.culture = CultureInfo.CurrentCulture;
            this.resourceLocation = resourceLocation;
        }

        /// <summary>
        /// Initializes new instance of localizer with given provider, resource file location and culture
        /// </summary>
        /// <param name="provider">The resource provider.</param>
        /// <param name="resourceLocation">The resource file location key ex. .data.file -> @localeroot/data/file.{culture.name}.json </param>
        /// <param name="culture">The culture </param>
        public UniLocalizer(IStorageProvider provider, string resourceLocation, CultureInfo culture)
        {
            this.provider = provider;
            this.resourceLocation = resourceLocation;
            this.culture = culture;
        }

        /// <summary>
        /// Gets localized value based on resouce token/key
        /// </summary>
        /// <param name="key">The resouce key</param>
        /// <returns>Localized string for given key</returns>
        public LocalizedString this[string key]
        {
            get
            {
                var value = GetString(key);
                return new LocalizedString(key, value ?? key, resourceNotFound: value == null);
            }
        }

        /// <summary>
        /// Gets localized value based on resouce token/key
        /// </summary>
        /// <param name="key">The resouce key</param>
        /// <returns>Localized string for given key</returns>
        public LocalizedString this[string key, params object[] arguments]
        {
            get
            {
                var format = GetString(key);
                var value = string.Format(format ?? key, arguments);
                return new LocalizedString(key, value, resourceNotFound: format == null);
            }
        }

        /// <summary>
        /// Gets localizer according to given culture.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Localizer instance.</returns>
        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new UniLocalizer(this.provider, this.resourceLocation, culture);
        }

        /// <summary>
        /// Gets all strings for current instace culture.
        /// </summary>
        /// <param name="includeAncestorCultures">Ignored</param>
        /// <returns>Collection of all localized strings for given culture</returns>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            var localizedStrings = this.provider.Get(this.culture);


            return localizedStrings.Select(r => new LocalizedString(r.Key, r.Value, true));
        }

        /// <summary>
        /// Gets localized string according to given key and this instace culture and resource location.
        /// </summary>
        /// <param name="key">The resource key / token</param>
        /// <returns>Localized string</returns>
        private string GetString(string key)
        {
            return this.provider.Get(this.culture, this.resourceLocation , key);
        }
    }
}
