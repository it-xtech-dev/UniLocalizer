using System.Globalization;
using Microsoft.Extensions.Localization;

namespace UniLocalizer;

/// <summary>
/// Culture-explicit lookups for code paths without <see cref="Microsoft.AspNetCore.Http.HttpContext"/>
/// (background services, startup, timers). Prefer storing resource keys during evaluation and
/// translating on the request thread when possible — see package README scenario 3.
/// </summary>
public static class UniLocalizerText
{
    /// <summary>
    /// Resolves a string for an explicit culture and resource location (not <see cref="CultureInfo.CurrentUICulture"/>).
    /// </summary>
    /// <param name="factory">Registered <see cref="UniLocalizerFactory"/>.</param>
    /// <param name="culture">Target culture (e.g. from user profile or <c>RequestLocalizationOptions</c> default).</param>
    /// <param name="resourceLocation">Dot path, e.g. <c>.pages.about</c> (leading dot optional).</param>
    /// <param name="resourceKey">Token key inside the JSON file.</param>
    public static string Get(
        IStringLocalizerFactory factory,
        CultureInfo culture,
        string resourceLocation,
        string resourceKey)
    {
        var uniFactory = (UniLocalizerFactory)factory;
        if (!resourceLocation.StartsWith('.'))
        {
            resourceLocation = "." + resourceLocation;
        }

        return uniFactory.Provider.Get(culture, resourceLocation, resourceKey);
    }

    /// <summary>
    /// Same as <see cref="Get(IStringLocalizerFactory, CultureInfo, string, string)"/> with culture name (e.g. <c>pl-PL</c>).
    /// </summary>
    public static string Get(
        IStringLocalizerFactory factory,
        string cultureName,
        string resourceLocation,
        string resourceKey) =>
        Get(factory, CultureInfo.GetCultureInfo(cultureName), resourceLocation, resourceKey);

    /// <summary>
    /// Builds an <see cref="IStringLocalizer"/> bound to a fixed culture (safe to keep as singleton per culture+location if needed).
    /// </summary>
    public static IStringLocalizer CreateLocalizer(
        IStringLocalizerFactory factory,
        CultureInfo culture,
        string resourceLocation)
    {
        var uniFactory = (UniLocalizerFactory)factory;
        if (!resourceLocation.StartsWith('.'))
        {
            resourceLocation = "." + resourceLocation;
        }

        return new UniLocalizer(uniFactory.Provider, resourceLocation, culture);
    }
}
