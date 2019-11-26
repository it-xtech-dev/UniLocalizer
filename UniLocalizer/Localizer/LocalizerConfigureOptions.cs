using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;

namespace UniLocalizer
{
    /// <summary>
    /// Enables static files serving from RCL area:
    /// https://www.learnrazorpages.com/advanced/razor-class-library
    /// https://stackoverflow.com/questions/51610513/can-razor-class-library-pack-static-files-js-css-etc-too
    /// </summary>
    internal class _LocalizerConfigureOptions : IPostConfigureOptions<StaticFileOptions>
    {
        
        private readonly IHostingEnvironment _environment;
        public _LocalizerConfigureOptions(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public void PostConfigure(string name, StaticFileOptions options)
        {
            // Basic initialization in case the options weren't initialized by any other component
            options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            if (options.FileProvider == null && _environment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }
            options.FileProvider = options.FileProvider ?? _environment.WebRootFileProvider;
            // Add our provider
            var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, "wwwroot");
            options.FileProvider = new CompositeFileProvider(options.FileProvider, filesProvider);
        }
    }
}
