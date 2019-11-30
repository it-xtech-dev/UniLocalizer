using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UniLocalizer.Localizer.Model;

namespace UniLocalizer.Providers
{

    /// <summary>
    /// Provides localizer json resource provider.
    /// </summary>
    public class SqlDbProvider: BaseProvider, IStorageProvider
    {
        LocalizerDbContext dbContext;

        /// <summary>
        /// Creates new instance of provider class.
        /// </summary>
        /// <param name="options">The options</param>
        public SqlDbProvider(ServiceOptions options, IMemoryCache cache, LocalizerDbContext dbContext): base(options, cache)
        {
            this.dbContext = dbContext;
            this.LoadResources();
        }

        /// <summary>
        /// Loads all resources from database to memory.
        /// </summary>
        private void LoadResources()
        {
            var dbResourceItems = this.dbContext.ResourceItems.Where(r => !r.LocationKey.StartsWith("$dynamic"));
            var resourceFiles = dbResourceItems.GroupBy(
                i => i.Culture + ":" + i.LocationKey,
                i => i,
                (key, g) => new { FileGeneralKey = key, Items = g.ToList() }
            );

            int index = 0;
            resourceFiles.OrderBy(f => f.FileGeneralKey).ToList().ForEach(file =>
            {
                var fileResourcesDictionary = file.Items.OrderBy(i => i.ResourceKey).ToDictionary(i => i.ResourceKey, i => i.Value);
                var json = JsonConvert.SerializeObject(fileResourcesDictionary, Formatting.Indented);

                var resourceFile = new ResourceFile(index, file.FileGeneralKey, json);

                resourceFile.OnSyncRequired += (sender, args) =>
                {
                    base.RefreshResourceFile(args.File);
                };
                // cache file content
                this.LoadedFiles.Add(resourceFile.KeyHashed, resourceFile);

                // load each of the items into memory (cache)
                file.Items.ToList().ForEach(i =>
                 {
                     var item = new ResourceItem(i.Culture, i.LocationKey, i.ResourceKey, i.Value, resourceFile);
                     this.AddResourceItem(item);
                 });

                 index++;
            });

        }


        /// <summary>
        /// Writes all resource from memory to json files. Performs save oparation according to cultureName and resourceFileKey
        /// </summary>
        /// <param name="cultureName">Saves files only with specified culture. The culture name is 5 letter formatted string: (ex: en-US). Use null to bypass this condition.</param>
        /// <param name="locationKey">Saves files only with specified resource file key (location). The value should be resourceFileKey pattern like data.filename -> data/filename.{culture.name}.json. Use null to bypass this condition.</param>
        public void WriteResources(string cultureName = null, string locationKey = null)
        {
            IEnumerable<ResourceFile> filesToSave = this.LoadedFiles.Values;

            if (locationKey != null)
            {
                filesToSave = filesToSave.Where(f => f.Key.EndsWith(locationKey));
            }
            if (cultureName != null)
            {
                filesToSave = filesToSave.Where(f => f.Key.StartsWith(cultureName));
            }

            // limit list only to files that has been modified
            List<ResourceFile> filesToSaveList = this.ItemList.Values.Where(i => i.IsModified).Select(i => i.File).Distinct().ToList();

            // iterate over the files and write them to storage
            filesToSaveList.ForEach(file => {

                var items = this.ItemList.Values.Where(i => i.GeneralKey.StartsWith(file.Key)).ToList();
                var dbToDelete = this.dbContext.ResourceItems.Where(dr => dr.CultureName == file.Culture.Name && dr.LocationKey == file.LocationKey);

                this.dbContext.RemoveRange(dbToDelete);

                var dbToInsert = items.Select(i => new DbResourceItem
                {
                    CultureName = i.Culture.Name,
                    LocationKey = i.LocationKey,
                    ResourceKey = i.Key,
                    Value = i.Value
                });

                this.dbContext.AddRange(dbToInsert);

                this.dbContext.SaveChanges();

                items.ForEach(i => i.IsModified = false);
            });
        }

        public void Reload()
        {
            base.Clear();
            this.LoadResources();
        }
    }
}
