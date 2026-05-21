using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using RazorPage.Services;
using UniLocalizer.Localizer.Model.Context;

namespace UniLocalizer
{
    /// <summary>
    /// Registers UniLocalizer in the host application DI container.
    /// </summary>
    /// <remarks>
    /// After registration, call <c>UseRequestLocalization</c> in the pipeline.
    /// Store <see cref="IStringLocalizerFactory"/> for background localization via <see cref="UniLocalizerText"/>
    /// or defer translation until a request is available (see package README).
    /// </remarks>
    public static class ServiceExtensions
    {

        ///// <summary>
        ///// Adds localizer serivce to application IoC container.
        ///// </summary>
        ///// <param name="services">The application services collection - passed over IoC injection</param>
        ///// <returns>ISeviceCollection for chaning</returns>
        //public static IServiceCollection AddUniLocalizer(
        //    this IServiceCollection services)
        //{
        //    return AddUniLocalizer(services, setupAction: null);
        //}


        /// <summary>
        /// Adds <see cref="UniLocalizerFactory"/> (singleton), default <see cref="UniLocalizer"/>, and optional SQL storage.
        /// </summary>
        /// <param name="services">Application services.</param>
        /// <param name="setupAction">Configures <see cref="ServiceOptions"/> (required: <see cref="ServiceOptions.ResourcesPath"/> for JSON).</param>
        /// <param name="connectionString">When set, registers <c>LocalizerDbContext</c> and uses <c>SqlDbProvider</c>; otherwise <c>JsonProvider</c>.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddUniLocalizer(
            this IServiceCollection services,
            Action<ServiceOptions> setupAction,
            string connectionString = null)
        {
            // Add db context to use database storage provider when connection string is configured
            if (connectionString != null)
            {

                services.AddDbContext<LocalizerDbContext>(
                    item => item.UseSqlServer(connectionString),
                    ServiceLifetime.Singleton,
                    ServiceLifetime.Singleton
                );
            }

            // Init localizer serv
            services.Add(new ServiceDescriptor(typeof(IStringLocalizerFactory),
                typeof(UniLocalizerFactory), ServiceLifetime.Singleton));
            services.Add(new ServiceDescriptor(typeof(IStringLocalizer),
                typeof(UniLocalizer), ServiceLifetime.Singleton));

            // TODO: CHECK - potentially could collide with app that will use this razor class lib
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // add razor page string renderer service
            services.AddTransient<IRazorToStringRenderer, RazorToStringRenderer>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            // Enable static files serving from Localizer area
            // https://www.learnrazorpages.com/advanced/razor-class-library
            services.ConfigureOptions(typeof(ServicePostConfigureOptions));

            return services;
        }
    }
}