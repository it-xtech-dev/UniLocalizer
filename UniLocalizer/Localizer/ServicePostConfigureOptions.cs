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
    /// Enables db context for localizer.
    /// Enables static files serving from RCL area:
    /// https://www.learnrazorpages.com/advanced/razor-class-library
    /// https://stackoverflow.com/questions/51610513/can-razor-class-library-pack-static-files-js-css-etc-too
    /// </summary>
    internal class ServicePostConfigureOptions : IPostConfigureOptions<StaticFileOptions>
    {
        
        private readonly IHostingEnvironment Enviroment;
        private readonly ServiceOptions UniLocalizerOptions;

        public ServicePostConfigureOptions(IHostingEnvironment environment, IOptions<ServiceOptions> options)
        {
            Enviroment = environment;
            UniLocalizerOptions = (ServiceOptions)options.Value;
        }

        public void PostConfigure(string name, StaticFileOptions staticFileOptions)
        {
            this.EnableRCLStaticContent(staticFileOptions);
        }

        private void EnableRCLStaticContent(StaticFileOptions staticFileOptions)
        {
            // Basic initialization in case the options weren't initialized by any other component
            staticFileOptions.ContentTypeProvider = staticFileOptions.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            if (staticFileOptions.FileProvider == null && Enviroment.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }
            staticFileOptions.FileProvider = staticFileOptions.FileProvider ?? Enviroment.WebRootFileProvider;
            // Add our provider
            var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, "wwwroot");
            staticFileOptions.FileProvider = new CompositeFileProvider(staticFileOptions.FileProvider, filesProvider);

        }
    }
}
