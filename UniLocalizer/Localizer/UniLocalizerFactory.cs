using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniLocalizer.Localizer.Model;
using UniLocalizer.Providers;

namespace UniLocalizer
{
    /// <summary>
    /// Provides uni localizer factory.
    /// </summary>
    public class UniLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IStorageProvider provider;
        public readonly ServiceOptions Options;

        /// <summary>
        /// Initializes new instace of localizer factory
        /// </summary>
        /// <param name="options">IoC injected localizer options</param>
        public UniLocalizerFactory(IOptions<ServiceOptions> options, IMemoryCache cache, IServiceProvider serviceProvider)
        {
            this.Options = options.Value;
            
            var dbContext = serviceProvider.GetService<LocalizerDbContext>();
            if (dbContext != null)
            {
                //Where no database context provided use sql db provider
                this.provider = new SqlDbProvider(options.Value, cache, dbContext);
            } 
            else
            {
                // Where no database context provided use json provider by default
                this.provider = new JsonProvider(options.Value, cache);

            }

        }

        /// <summary>
        /// Creates new instance of localizer based on resouce type.
        /// </summary>
        /// <param name="resourceSource">The type of class for corresponding resource</param>
        /// <returns></returns>
        public IStringLocalizer Create(Type resourceSource)
        {
            var fileResourceKey = "." + resourceSource.FullName.ToLower();

            return new UniLocalizer(this.Provider, fileResourceKey);
        }

        /// <summary>
        /// Creates new instance of localizer based on requested entity location.
        /// </summary>
        /// <param name="baseName">The location full key/path specified as dot separated path</param>
        /// <param name="location">The location root key/path</param>
        /// <returns></returns>
        public IStringLocalizer Create(string baseName, string location)
        {
            var locationKey = new string(baseName.Skip(location.Length).ToArray());
            return new UniLocalizer(this.Provider, locationKey.ToLower());
        }

        /// <summary>
        /// Creates new instance of localizer based on file resouce key.
        /// </summary>
        /// <param name="fileResourceKey">The file resouce key specified as dot separated path and file name. Ex: data/file => data.file{culture.name}.json </param>
        /// <returns></returns>
        public IStringLocalizer Create(string fileResourceKey)
        {
            if (!fileResourceKey.StartsWith(".")) fileResourceKey = "." + fileResourceKey;

            return new UniLocalizer(this.Provider, fileResourceKey);
        }

        /// <summary>
        /// Gets current instace localizer provider.
        /// </summary>
        public IStorageProvider Provider
        {
            get { return this.provider;  }
        }
    }
}
