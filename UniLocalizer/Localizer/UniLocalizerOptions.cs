using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.IO;

namespace UniLocalizer
{
    /// <summary>
    /// Provides localizer options.
    /// </summary>
    public class UniLocalizerOptions : LocalizationOptions
    {
        public UniLocalizerOptions()
        {
        }

        /// <summary>
        /// Determines whether localizer key request that did not hit any item should generate keys inside dictionary.
        /// </summary>
        public bool AutogenerateMissingKeys
        {
            get; set;
        } = true;

        /// <summary>
        /// Gets or sets whether localizer should show sort or long token notation.
        /// </summary>
        public bool DisplayShortNotation
        {
            get; set;
        } = true;

        /// <summary>
        /// Gets or set javascript namespace for culture dependent resources. Specify as dot separated string eg: App.Locale. When different than 'locale' is exepcted to exist during lib initialization.
        /// </summary>
        public string JavascriptNamespace
        {
            get; set;
        } = "locale";

        /// <summary>
        /// Determines whether translator page and its utils are enabled. Default: false
        /// NOTE: In general for production enviroment this should be set to false.
        /// </summary>
        public bool IsTranslatorEnabled
        {
            get; set;
        } = false;

        /// <summary>
        /// Determines whether translator view and its utils are restricted only to user with particular role. 
        /// Default value: "Translator".
        /// NOTE: 
        ///     1. This setting is evaluated only when IsTranslatorEnabled is set to true.
        ///     2. Use null to remove restriction or set role name that should have access.
        /// </summary>
        public string TraslatorUserRole
        {
            get; set;
        } = "Translator";
    }
}
