namespace UniLocalizer.Demo
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Models;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(item => item.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // DEMO:
            // Register uni localizer as localization service with some settings
            // below configuration is json based provider:
            services.AddUniLocalizer(opt =>
                {
                    opt.ResourcesPath = Directory.GetCurrentDirectory() + @"\wwwroot\locale\";
                    opt.AutogenerateMissingKeys = true;
                    opt.DisplayShortNotation = true;
                    opt.JavascriptNamespace = "locale";
                    opt.IsTranslatorEnabled = true;
                    opt.TraslatorUserRole = null;
                }
                // Enables database sourced translations. When null, json files included into project structure will be the source of the translations.
                // , Configuration.GetConnectionString("DefaultConnection")
            );


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                // DEMO: CheckConsentNeeded is set to false because theres no other way to save language settings
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            });

            services.AddRazorPages();



            services
                .AddMvc(opt => opt.EnableEndpointRouting = false)
                // DEMO:
                // Enable view localization
                .AddViewLocalization()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // DEMO:
            // General language support
            // Localization implementation based on:
            // https://andrewlock.net/adding-localisation-to-an-asp-net-core-application/
            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var cultures = new string[] { "pl-PL","en-US" };

                // Read direcly from appsettings:
                //Configuration.GetSection("SupportedCultures").GetChildren().Select(c => c.Value).ToList();              

                opts.DefaultRequestCulture = new RequestCulture(cultures.FirstOrDefault());
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = cultures.Select(c => new CultureInfo(c)).ToArray();
                // UI strings that we have localized.
                opts.SupportedUICultures = cultures.Select(c => new CultureInfo(c)).ToArray();

                opts.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CookieRequestCultureProvider()
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // DEMO:
            // Use localization features
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
