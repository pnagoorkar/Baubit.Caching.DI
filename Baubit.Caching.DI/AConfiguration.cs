using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Caching.DI
{
    public abstract class AConfiguration : Baubit.DI.AConfiguration
    {
        public bool IncludeL1Caching { get; set; }
        public int L1MinCap { get; set; } = 128;
        public int L1MaxCap { get; set; } = 8192;
        public Configuration CacheConfiguration { get; set; }
        public ServiceLifetime CacheLifetime { get; set; } = ServiceLifetime.Singleton;
    }
}
