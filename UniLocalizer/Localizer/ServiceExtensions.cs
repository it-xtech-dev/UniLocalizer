using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using RazorPage.Services;
using UniLocalizer.Localizer.Model;

namespace UniLocalizer
{
    /// <summary>
    /// Provides localizer application service extensions
    /// </summary>
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
        /// Adds localizer serivce with optional configuration to application IoC container.
        /// </summary>
        /// <param name="services">The application services collection - passed over IoC injection</param>
        /// <param name="setupAction">The localizer options configuration action.</param>
        /// <returns>ISeviceCollection for chaning</returns>
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