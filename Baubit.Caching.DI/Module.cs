using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI
{
    /// <summary>
    /// Abstract base module for registering an ordered cache in dependency injection.
    /// Provides a template for building ordered caches with configurable L1/L2 data stores and metadata.
    /// </summary>
    /// <typeparam name="TId">The type of IDs used to identify cache entries.</typeparam>
    /// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
    /// <typeparam name="TConfiguration">The configuration type, must derive from <see cref="Configuration"/>.</typeparam>
    public abstract class Module<TId, TValue, TConfiguration> : Baubit.DI.Module<TConfiguration> where TConfiguration : Configuration where TId : struct, IComparable<TId>, IEquatable<TId>
    {
        /// <summary>
        /// Initializes a new instance of the module class
        /// using an <see cref="IConfiguration"/> to bind settings.
        /// </summary>
        /// <param name="configuration">The configuration section to bind to <typeparamref name="TConfiguration"/>.</param>
        protected Module(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the module class
        /// using an explicit configuration object and optional nested modules.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="nestedModules">Optional list of nested modules to load.</param>
        protected Module(TConfiguration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        /// <summary>
        /// Loads the ordered cache into the service collection
        /// with the configured <see cref="Configuration.CacheLifetime"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <exception cref="NotImplementedException">Thrown when an unsupported <see cref="ServiceLifetime"/> is specified.</exception>
        public override void Load(IServiceCollection services)
        {
            switch (Configuration.CacheLifetime)
            {
                case ServiceLifetime.Singleton:
                    if (string.IsNullOrEmpty(Configuration.RegistrationKey))
                    {
                        services.AddSingleton(BuildOrderedCache);
                    }
                    else
                    {
                        services.AddKeyedSingleton(serviceKey: Configuration.RegistrationKey, (serviceProvider, _) => BuildOrderedCache(serviceProvider));
                    }
                    break;
                case ServiceLifetime.Transient:
                    if (string.IsNullOrEmpty(Configuration.RegistrationKey))
                    {
                        services.AddTransient(BuildOrderedCache);
                    }
                    else
                    {
                        services.AddKeyedTransient(serviceKey: Configuration.RegistrationKey, (serviceProvider, _) => BuildOrderedCache(serviceProvider));
                    }
                    break;
                case ServiceLifetime.Scoped:
                    if (string.IsNullOrEmpty(Configuration.RegistrationKey))
                    {
                        services.AddScoped(BuildOrderedCache);
                    }
                    else
                    {
                        services.AddKeyedScoped(serviceKey: Configuration.RegistrationKey, (serviceProvider, _) => BuildOrderedCache(serviceProvider));
                    }
                    break;
                default: throw new NotImplementedException();
            }
            base.Load(services);
        }

        /// <summary>
        /// Builds the ordered cache instance using the configured stores and metadata.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>A configured ordered cache instance.</returns>
        protected virtual IOrderedCache<TId, TValue> BuildOrderedCache(IServiceProvider serviceProvider)
        {
            return new OrderedCache<TId, TValue>(Configuration.CacheConfiguration,
                                            Configuration.IncludeL1Caching ? BuildL1DataStore(serviceProvider) : null,
                                            BuildL2DataStore(serviceProvider),
                                            BuildMetadata(serviceProvider),
                                            serviceProvider.GetRequiredService<ILoggerFactory>(),
                                            () => new CacheEnumeratorCollection<TId>(),
                                            new CacheAsyncEnumeratorFactory<TId, TValue>());
        }

        /// <summary>
        /// Builds the L1 (fast lookup) data store when L1 caching is enabled.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>A data store for L1 caching.</returns>
        protected abstract IStore<TId, TValue> BuildL1DataStore(IServiceProvider serviceProvider);

        /// <summary>
        /// Builds the L2 (primary) data store.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>A data store for L2 storage.</returns>
        protected abstract IStore<TId, TValue> BuildL2DataStore(IServiceProvider serviceProvider);

        /// <summary>
        /// Builds the metadata store for tracking cache entries.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>A metadata store instance.</returns>
        protected abstract IMetadata<TId> BuildMetadata(IServiceProvider serviceProvider);
    }
}
