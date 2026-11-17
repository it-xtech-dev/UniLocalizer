using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using UniLocalizer.Localizer.Model.Context;
using UniLocalizer.Providers;

namespace UniLocalizer
{
    /// <summary>
    /// <see cref="IStringLocalizerFactory"/> implementation. Exposes <see cref="Provider"/> for culture-explicit lookups.
    /// </summary>
    /// <remarks>
    /// <para><b>Request-bound:</b> <see cref="Create(string)"/> → <see cref="UniLocalizer"/> uses <see cref="CultureInfo.CurrentCulture"/> at construction.</para>
    /// <para><b>No HttpContext:</b> use <see cref="Provider"/>.<see cref="IStorageProvider.Get(CultureInfo, string, string)"/>
    /// or <see cref="UniLocalizerText.Get(IStringLocalizerFactory, CultureInfo, string, string)"/>.</para>
    /// </remarks>
    public class UniLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IStorageProvider provider;
        public readonly ServiceOptions Options;

        /// <summary>
        /// Initializes new instace of localizer factory
        /// </summary>
        /// <param name="options">IoC injected localizer options</param>
        /// <param name="cache">The cache</param>
        /// <param name="serviceProvider">The service provider</param>
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
            var locationKey = new string([.. baseName.Skip(location.Length)]);
            return new UniLocalizer(this.Provider, locationKey.ToLower());
        }

        /// <summary>
        /// Creates a localizer for a JSON resource location (dot path, leading dot optional).
        /// </summary>
        /// <param name="fileResourceKey">e.g. <c>.pages.about</c> → <c>locale/pages/about.{culture}.json</c>.</param>
        /// <remarks>Culture is captured from <see cref="CultureInfo.CurrentCulture"/> at call time — not suitable for background threads.</remarks>
        public IStringLocalizer Create(string fileResourceKey)
        {
            if (!fileResourceKey.StartsWith('.')) fileResourceKey = "." + fileResourceKey;

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
