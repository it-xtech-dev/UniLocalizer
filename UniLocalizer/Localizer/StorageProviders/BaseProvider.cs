﻿using Microsoft.Extensions.Caching.Memory;
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
using System.Threading;


/// <summary>
/// TODO: Major refactoring:
/// 1. Remove KeyHash in favor for Key (KeyHash is no longer required)
/// 2. 
/// 3. Evaluate error prones of using file Index props (some files might not have same index over time). Loading files in different orde
/// </summary>
namespace UniLocalizer.Providers
{

    /// <summary>
    /// Provides localizer json resource provider.
    /// </summary>
    public class BaseProvider
    {
        internal string resourceRootPath;
        internal IMemoryCache cache;

        /// <summary>
        /// Creates new instance of provider class.
        /// </summary>
        /// <param name="options">The options</param>
        public BaseProvider(ServiceOptions options, IMemoryCache cache)
        {
            this.Options = options;
            this.resourceRootPath = options.ResourcesPath;
            this.cache = cache;
            //this.LoadResources(this.resourceRootPath);
        }

        public ServiceOptions Options
        {
            get; private set;
        }

        /// <summary>
        /// Clears  all resources form files.
        /// </summary>
        internal void Clear()
        {
            this.ItemList.Clear();
            this.LoadedFiles.Clear();
            //this.LoadResources(this.resourceRootPath);
        }

        /// <summary>
        /// Gets list of currently loaded resource items (translation tokens).
        /// </summary>
        public Dictionary<string, ResourceItem> ItemList { get; } = new Dictionary<string, ResourceItem>();

        /// <summary>
        /// Gets list of currently loaded files.
        /// </summary>
        public Dictionary<string, ResourceFile> LoadedFiles { get; } = new Dictionary<string, ResourceFile>();

        /// <summary>
        /// Gets localized string for given culture, resource file locatio and resource key.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="resourceLocation">The resouce file location (root relative) ex: path1.path2.fileName </param>
        /// <param name="resourceKey">The resouce key</param>
        /// <returns>Localized string</returns>
        public string Get(CultureInfo culture, string resourceLocation, string resourceKey)
        {
            return InternalGet(culture, resourceLocation, resourceKey);
        }

        /// <summary>
        /// Gets localized string for given general key
        /// </summary>
        /// <param name="generalKey">General key containg of culture, resource file location and resource key. ex: en-US:.data/file1:My_Key </param>
        /// <returns>Localized string</returns>
        public string Get(string generalKey)
        {
            return InternalGet(generalKey);
        }

        /// <summary>
        /// Gets collection of localized strings for given culture.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Dictionary of key / value localized strings.</returns>
        public Dictionary<string, string> Get(CultureInfo culture)
        {
            return InternalGet(culture);
        }

        /// <summary>
        /// Gets collection of localized strings for given culture.
        /// <param name="culture">The culture</param>
        /// <returns>NotImplementedException</returns>
        private Dictionary<string,string> InternalGet(CultureInfo culture)
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets localized string for given general key.
        /// </summary>
        /// <param name="generalKey">General key containg of culture, resource file location and resource key. ex: en-US:.data/file1:My_Key </param>
        /// <returns>Localized string when found or missing/autogenerated text</returns>
        private string InternalGet(string generalKey)
        {
            var localizedString = this.ItemList.GetValue(generalKey.GetHashString())?.Value;
            const string AUTO_GEN = "#AUTOGENERATED";

            var keys = generalKey.Split(':');
            var culture = new CultureInfo(keys[0]);
            var resourceLocationKey = keys[1];
            var itemKey = keys[2];

            if (localizedString == null && Options.AutogenerateMissingKeys && Options.IsTranslatorEnabled)
            {
                var item = this.CreateResourceItem(culture, resourceLocationKey, itemKey, AUTO_GEN);
                localizedString = item.Value;
            }
            if (localizedString == null)
            {
                return "#MISS:" + (this.Options.DisplayShortNotation ?  itemKey : generalKey);
            }
            if (localizedString == "#AUTOGENERATED")
            {
                return "#AUTO" + ":" + (this.Options.DisplayShortNotation ? itemKey : generalKey);
            }

            return localizedString;
        }


        /// <summary>
        /// Gets localized string for given culture, resouce file location key and resource key.
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <param name="resourceLocationKey">The resouce location key, ex: .data.fileName </param>
        /// <param name="itemKey">The resource key.</param>
        /// <returns></returns>
        private string InternalGet(CultureInfo culture, string resourceLocationKey, string itemKey)
        {
            string generalKey = culture.Name + ":" + resourceLocationKey + ":" + itemKey;
            return this.InternalGet(generalKey);
        }

        /// <summary>
        /// Creates new resource item and adds it to items collection.
        /// </summary>
        /// <param name="culture">The culture for resource.</param>
        /// <param name="locationKey">The resource location key base on resource path </param>
        /// <param name="resourceKey"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal ResourceItem CreateResourceItem(CultureInfo culture, string locationKey, string resourceKey, string text)
        {
            var fileGeneralKey = culture + ":" + locationKey + ":";

            var fileKeyHash = ResourceFile.GetKeyHash(fileGeneralKey);
            if (!this.LoadedFiles.TryGetValue(fileKeyHash, out var file))
            {
                // it seems that file for this token does not exits
                // we have to add it manuall - file will be saved on users demand
                var maxFileIndex = 0;
                if (this.LoadedFiles.Count > 0)
                {
                    maxFileIndex = this.LoadedFiles.Values.Max(f => f.Index);
                }
                // file content will be generated later;
                file = new ResourceFile(maxFileIndex + 1, fileGeneralKey, "");

                // TODO: use Logger to log when add was unsuccessfull.
                // There might be some issues realted to multithreading.
                this.LoadedFiles.TryAdd(fileKeyHash, file);
            }
            var item = new ResourceItem(culture, locationKey, resourceKey, null, file);
            // TODO: use Logger to log when add was unsuccessfull.
            // There might be some issues realted to multithreading.
            this.AddResourceItem(item);

            // trigger resource modification for change tracking.
            item.Value = text;

            return item;
        }

        /// <summary>
        /// Adds resource item object to items list, registers modfication handler before adding.
        /// </summary>
        /// <param name="item">The resource item</param>
        internal bool AddResourceItem(ResourceItem item)
        {
            item.OnModification += (sender, eventArgs) =>
            {
                var cacheFileItemKey = $"Localizer_File_{item.File.Index}";
                var tokenSource = this.cache.Get<CancellationTokenSource>(cacheFileItemKey);
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    cache.Remove(cacheFileItemKey);
                }
            };
            return this.ItemList.TryAdd(item.GeneralKey.GetHashString(), item);
        }

        /// <summary>
        /// Refreshes resource file content according to current ResourceItems state.
        /// </summary>
        /// <param name="file">The resource file object</param>
        internal void RefreshResourceFile(ResourceFile file)
        {
            var fileKey = file.Key;
            var resourceItems = this.ItemList.Where(i => i.Value.GeneralKey.StartsWith(fileKey));
            var resourceItemsDictionary = resourceItems.OrderBy(i => i.Value.Key).ToDictionary(i => i.Value.Key, i => i.Value.Value);
            var json = JsonConvert.SerializeObject(resourceItemsDictionary, Formatting.Indented);
            file.Content = json;

            file.IsSynced = true;
        }


        /// <summary>
        /// Adds new or updates existing resource.
        /// </summary>
        /// <param name="culture">The resource culture.</param>
        /// <param name="locationKey">The resource location (path).</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="text">The resource text.</param>
        public ResourceItem AddOrUpdate(CultureInfo culture, string locationKey, string resourceKey, string text)
        {
            string generalKey = culture.Name + ":" + locationKey + ":" + resourceKey;
            if (!this.ItemList.TryGetValue(generalKey, out var resourceItem)) {
                this.CreateResourceItem(culture, locationKey, resourceKey, text);
            } 
            else
            {
                resourceItem.Value = text;
            }

            return resourceItem;
        }
    }
}