#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER

using Microsoft.Extensions.DependencyInjection;

namespace Forge.Formatters
{

    /// <summary>Add formatters as singletons</summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>Adds the forge formatters.</summary>
        /// <param name="services">The services.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static IServiceCollection AddForgeFormatters(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAesByteArrayFormatter, AesByteArrayFormatter>()
                .AddSingleton<IGZipFormatter, GZipFormatter>()
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                .AddSingleton<IBrotliFormatter, BrotliFormatter>()
#endif
                ;
        }

    }

}

#endif