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

namespace UniLocalizer
{

    /// <summary>
    /// Provides localizer json resource provider.
    /// </summary>
    public interface IUniLocalizerStorageProvider
    {
        /// <summary>
        /// Writes all resource from memory to json files. Performs save oparation according to cultureName and resourceFileKey
        /// </summary>
        /// <param name="cultureName">Saves files only with specified culture. The culture name is 5 letter formatted string: (ex: en-US). Use null to bypass this condition.</param>
        /// <param name="resourceFileKey">Saves files only with specified resource file key (location). The value should be resourceFileKey pattern like data.filename -> data/filename.{culture.name}.json. Use null to bypass this condition.</param>
        void WriteResources(string cultureName = null, string resourceFileKey = null);

        /// <summary>
        /// Reloads all resources form files.
        /// </summary>
        void Reload();

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
        /// Gets localized string for given culture, resource file locatio and resource key.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="resourceLocation">The resouce file location (root relative) ex: path1.path2.fileName </param>
        /// <param name="resourceKey">The resouce key</param>
        /// <returns>Localized string</returns>
        string Get(CultureInfo culture, string resourceLocation, string resourceKey);

        /// <summary>
        /// Gets localized string for given general key
        /// </summary>
        /// <param name="generalKey">General key containg of culture, resource file location and resource key. ex: en-US:.data/file1:My_Key </param>
        /// <returns>Localized string</returns>
        string Get(string generalKey);

        /// <summary>
        /// Gets collection of localized strings for given culture.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Dictionary of key / value localized strings.</returns>
        Dictionary<string, string> Get(CultureInfo culture);

        /// <summary>
        /// Adds new resource item to memory cache.
        /// </summary>
        /// <param name="item">The resource item</param>
        void Add(ResourceItem item);
    }
}
