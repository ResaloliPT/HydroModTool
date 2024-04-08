using HydroToolChain.App.UE4Pak;
using HydroToolChain.App.UE4Pak.Paker;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddUE4Paker(this IServiceCollection services, UE4PakVersion pakVersion)
        {
            services.Configure<UE4PakerOptions>(opt =>
            {
                opt.PakVersion = pakVersion;
            });

            services.AddTransient<IPaker>(_ => pakVersion switch
            {
                UE4PakVersion.ONE => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.THREE => new PakerV3(),
                UE4PakVersion.TWO => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.FOUR => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.FIVE => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.SIX => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.SEVEN => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.EIGHT => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.NINE => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.TEN => throw new NotSupportedException("Only pak version 3 is implemented"),
                UE4PakVersion.ELEVEN => throw new NotSupportedException("Only pak version 3 is implemented"),
                _ => throw new ArgumentOutOfRangeException(nameof(pakVersion), pakVersion, null)
            });

            return services;
        }
    }
}