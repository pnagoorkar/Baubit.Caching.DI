using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Caching.DI
{
    /// <summary>
    /// Abstract base configuration for caching modules.
    /// Provides common settings for L1 caching and service lifetime.
    /// </summary>
    public abstract class AConfiguration : Baubit.DI.AConfiguration
    {
        /// <summary>
        /// Gets or sets whether to include L1 (in-memory) caching layer.
        /// When enabled, a bounded in-memory store is used as a fast lookup layer.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool IncludeL1Caching { get; set; }

        /// <summary>
        /// Gets or sets the minimum capacity for the L1 cache store.
        /// Only used when <see cref="IncludeL1Caching"/> is <c>true</c>.
        /// Defaults to 128.
        /// </summary>
        public int L1MinCap { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum capacity for the L1 cache store.
        /// Only used when <see cref="IncludeL1Caching"/> is <c>true</c>.
        /// Defaults to 8192.
        /// </summary>
        public int L1MaxCap { get; set; } = 8192;

        /// <summary>
        /// Gets or sets the underlying <see cref="Baubit.Caching.Configuration"/> for the ordered cache.
        /// </summary>
        public Configuration CacheConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the service lifetime for the registered <see cref="IOrderedCache{TValue}"/>.
        /// Defaults to <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public ServiceLifetime CacheLifetime { get; set; } = ServiceLifetime.Singleton;

        public string RegistrationKey { get; set; } = null;
    }
}
