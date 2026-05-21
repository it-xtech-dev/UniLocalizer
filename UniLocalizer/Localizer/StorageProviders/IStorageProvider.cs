using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UniLocalizer.Providers
{

    /// <summary>
    /// In-memory translation store (JSON or SQL). Thread-safe reads; use explicit <paramref name="culture"/> off the request thread.
    /// </summary>
    public interface IStorageProvider
    {
        ServiceOptions Options
        {
            get;
        }

        /// <summary>
        /// Gets list of currently loaded resource items (tokens).
        /// </summary>
        Dictionary<string, ResourceItem> ItemList
        {
            get;
        }

        /// <summary>
        /// Gets list of currently loaded files.
        /// </summary>
        Dictionary<string, ResourceFile> LoadedFiles
        {
            get;
        }

        /// <summary>
        /// Gets collection of localized strings for given culture.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Dictionary of key / value localized strings.</returns>
        Dictionary<string, string> Get(CultureInfo culture);

        /// <summary>
        /// Resolves one token. Preferred API when <see cref="CultureInfo.CurrentUICulture"/> is unavailable or wrong.
        /// </summary>
        /// <param name="culture">Explicit culture (not inferred from HTTP).</param>
        /// <param name="resourceLocation">Dot path, e.g. <c>.pages.about</c>.</param>
        /// <param name="resourceKey">JSON property name.</param>
        /// <returns>Translated text, or <c>#MISS:key</c> / <c>#AUTO:key</c> when missing (see <see cref="ServiceOptions.AutogenerateMissingKeys"/>).</returns>
        string Get(CultureInfo culture, string resourceLocation, string resourceKey);

        /// <summary>
        /// Gets localized string for given general key
        /// </summary>
        /// <param name="generalKey">General key containg of culture, resource file location and resource key. ex: en-US:.data/file1:My_Key </param>
        /// <returns>Localized string</returns>
        string Get(string generalKey);

        /// <summary>
        /// Writes all resource from memory to json files. Performs save oparation according to cultureName and resourceFileKey
        /// </summary>
        /// <param name="cultureName">Saves files only with specified culture. The culture name is 5 letter formatted string: (ex: en-US). Use null to bypass this condition.</param>
        /// <param name="resourceFileKey">Saves files only with specified resource file key (location). The value should be resourceFileKey pattern like data.filename -> data/filename.{culture.name}.json. Use null to bypass this condition.</param>
        void WriteResources(string cultureName = null, string resourceFileKey = null);


        /// <summary>
        /// Clears caches and reloads resources from the storage
        /// </summary>
        void Reload();

    }
}
