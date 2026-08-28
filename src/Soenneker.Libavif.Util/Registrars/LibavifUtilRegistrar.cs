using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Process.Registrars;

namespace Soenneker.Libavif.Util.Registrars;

public static class LibavifUtilRegistrar
{
    /// <summary>Adds <see cref="ILibavifUtil"/> as a singleton service.</summary>
    public static IServiceCollection AddLibavifUtilAsSingleton(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsSingleton()
                .AddFileUtilAsSingleton()
                .AddProcessUtilAsSingleton()
                .TryAddSingleton<ILibavifUtil, LibavifUtil>();

        return services;
    }

    /// <summary>Adds <see cref="ILibavifUtil"/> as a scoped service.</summary>
    public static IServiceCollection AddLibavifUtilAsScoped(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsScoped()
                .AddFileUtilAsScoped()
                .AddProcessUtilAsScoped()
                .TryAddScoped<ILibavifUtil, LibavifUtil>();

        return services;
    }
}
