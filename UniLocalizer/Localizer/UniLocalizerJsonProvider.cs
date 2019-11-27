﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace UniLocalizer
{

    /// <summary>
    /// Provides localizer json resource provider.
    /// </summary>
    public class UniLocalizerJsonProvider: UniLocalizerBaseProvider, IUniLocalizerStorageProvider
    {
        /// <summary>
        /// Creates new instance of provider class.
        /// </summary>
        /// <param name="options">The options</param>
        public UniLocalizerJsonProvider(IOptions<UniLocalizerOptions> options, IMemoryCache cache) : base(options, cache)
        {
            this.LoadResources(base.resourceRootPath);
        }

        /// <summary>
        /// Loads all resources from files to memory.
        /// </summary>
        /// <param name="resourceRootPath">The root path that will be searched for resource files.</param>
        private void LoadResources(string resourceRootPath)
        {
            var resourceFiles = Directory.GetFiles(resourceRootPath, "*.json", SearchOption.AllDirectories);
            var index = 0;
            resourceFiles.ToList().ForEach(file =>
            {
                var fileInfo = new FileInfo(file);
                // extract culture form file name
                var culture = new CultureInfo(new string(fileInfo.Name.TakeLast(10).Take(5).ToArray()));
                // extract dot separated root relative path for resource file
                var fileRelativePath = Path.GetRelativePath(resourceRootPath, Path.GetDirectoryName(file));
                var filePathKey = fileRelativePath.Replace(@"\", ".").ToLower();
                // create file resource location key
                string resourceLocationKey = "." + filePathKey + "." + new string(fileInfo.Name.SkipLast(11).ToArray());
                // get json content for file
                var json = File.ReadAllText(file, Encoding.UTF8);

                // create general key for resource file containing of culture + reource file key
                var fileGeneralKey = culture.Name + ":" + resourceLocationKey + ":";
                var resourceFile = new ResourceFile(index, fileGeneralKey, json);

                resourceFile.OnSyncRequired += (sender, args) => {
                    base.RefreshResourceFile(args.File);
                };
                // cache file content
                this.LoadedFiles.Add(resourceFile.KeyHashed, resourceFile);

                // import file resouce items to dictionary.
                var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                // load each of the items into memory (cache)
                items.ToList().ForEach(i =>
                {
                    var item = new ResourceItem(culture, resourceLocationKey, i.Key, i.Value, resourceFile);
                    this.Add(item);
                });

                index++;
            });

            //TODO: Invalidate cache ones resources loaded.
        }


        /// <summary>
        /// Writes all resource from memory to json files. Performs save oparation according to cultureName and resourceFileKey
        /// </summary>
        /// <param name="cultureName">Saves files only with specified culture. The culture name is 5 letter formatted string: (ex: en-US). Use null to bypass this condition.</param>
        /// <param name="resourceFileKey">Saves files only with specified resource file key (location). The value should be resourceFileKey pattern like data.filename -> data/filename.{culture.name}.json. Use null to bypass this condition.</param>
        public void WriteResources(string cultureName = null, string resourceFileKey = null)
        {
            IEnumerable<ResourceFile> filesToSave = this.LoadedFiles.Values;
            
            if (resourceFileKey != null)
            {
                filesToSave = filesToSave.Where(f => f.Key == resourceFileKey);
            }
            if (cultureName != null)
            {
                filesToSave = filesToSave.Where(f => f.Key.StartsWith(cultureName));
            }

            filesToSave.ToList().ForEach( file => {
                // when no changes detected skip data saving.
                var fileItems = this.ItemList.Values.Where(i => i.IsModified && i.GeneralKey.StartsWith(file.Key));
                
                if (fileItems.Any())
                {
                    var fileItemsDictionary = fileItems.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
                    var json = JsonConvert.SerializeObject(fileItemsDictionary, Formatting.Indented);

                    var filePath = options.ResourcesPath + file.RelativePath;
                    File.WriteAllText(filePath, json);

                    // reset modification indicator.
                    fileItems.ToList().ForEach(i => i.IsModified = false);
                }
            });
        }

        /// <summary>
        /// Reloads all resources form files.
        /// </summary>
        public void Reload()
        {
            base.Clear();
            this.LoadResources(this.resourceRootPath);
        }
    }
}
