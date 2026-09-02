using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Process.Registrars;
using Soenneker.Utils.Paths.Resources.Registrars;

namespace Soenneker.Libavif.Util.Registrars;

public static class LibavifUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ILibavifUtil"/> as a singleton service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddLibavifUtilAsSingleton(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsSingleton()
                .AddFileUtilAsSingleton()
                .AddProcessUtilAsSingleton()
                .AddResourcesPathUtilAsSingleton()
                .TryAddSingleton<ILibavifUtil, LibavifUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ILibavifUtil"/> as a scoped service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddLibavifUtilAsScoped(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsScoped()
                .AddFileUtilAsScoped()
                .AddProcessUtilAsScoped()
                .AddResourcesPathUtilAsScoped()
                .TryAddScoped<ILibavifUtil, LibavifUtil>();

        return services;
    }
}
