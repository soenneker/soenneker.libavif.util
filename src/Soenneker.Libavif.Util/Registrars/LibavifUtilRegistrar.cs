using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Libavif.Util.Abstract;
using Soenneker.Utils.Process.Registrars;

namespace Soenneker.Libavif.Util.Registrars;

public static class LibavifUtilRegistrar
{
    /// <summary>Adds <see cref="ILibavifUtil"/> as a singleton service.</summary>
    public static IServiceCollection AddLibavifUtilAsSingleton(this IServiceCollection services)
    {
        services.AddProcessUtilAsSingleton();
        services.TryAddSingleton<ILibavifUtil, LibavifUtil>();
        return services;
    }
}
