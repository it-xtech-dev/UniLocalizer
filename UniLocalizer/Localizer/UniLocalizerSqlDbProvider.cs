using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;


namespace UniLocalizer
{

    /// <summary>
    /// Provides localizer json resource provider.
    /// </summary>
    public class UniLocalizerSqlDbProvider: UniLocalizerBaseProvider, IUniLocalizerStorageProvider
    {

        /// <summary>
        /// Creates new instance of provider class.
        /// </summary>
        /// <param name="options">The options</param>
        public UniLocalizerSqlDbProvider(IOptions<UniLocalizerOptions> options, IMemoryCache cache): base(options, cache)
        {
            this.options = options;
        }

        /// <summary>
        /// Loads all resources from database to memory.
        /// </summary>
        private void LoadResources()
        {
        }


        /// <summary>
        /// Writes all resource from memory to json files. Performs save oparation according to cultureName and resourceFileKey
        /// </summary>
        /// <param name="cultureName">Saves files only with specified culture. The culture name is 5 letter formatted string: (ex: en-US). Use null to bypass this condition.</param>
        /// <param name="resourceFileKey">Saves files only with specified resource file key (location). The value should be resourceFileKey pattern like data.filename -> data/filename.{culture.name}.json. Use null to bypass this condition.</param>
        public void WriteResources(string cultureName = null, string resourceFileKey = null)
        {
        }

        /// <summary>
        /// Reloads all resources form files.
        /// </summary>
        public void Reload()
        {
            this.Clear();
            this.LoadResources();
        }
    }
}
