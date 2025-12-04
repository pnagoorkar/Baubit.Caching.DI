using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI
{
    /// <summary>
    /// Abstract base module for registering <see cref="IOrderedCache{TValue}"/> in dependency injection.
    /// Provides a template for building ordered caches with configurable L1/L2 data stores and metadata.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
    /// <typeparam name="TConfiguration">The configuration type, must derive from <see cref="AConfiguration"/>.</typeparam>
    public abstract class AModule<TValue, TConfiguration> : Baubit.DI.AModule<TConfiguration> where TConfiguration : AConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AModule{TValue, TConfiguration}"/> class
        /// using an <see cref="IConfiguration"/> to bind settings.
        /// </summary>
        /// <param name="configuration">The configuration section to bind to <typeparamref name="TConfiguration"/>.</param>
        protected AModule(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AModule{TValue, TConfiguration}"/> class
        /// using an explicit configuration object and optional nested modules.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="nestedModules">Optional list of nested modules to load.</param>
        protected AModule(TConfiguration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        /// <summary>
        /// Loads the <see cref="IOrderedCache{TValue}"/> into the service collection
        /// with the configured <see cref="AConfiguration.CacheLifetime"/>.
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
        /// Builds the <see cref="IOrderedCache{TValue}"/> instance using the configured stores and metadata.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>A configured <see cref="IOrderedCache{TValue}"/> instance.</returns>
        private IOrderedCache<TValue> BuildOrderedCache(IServiceProvider serviceProvider)
        {
            return new OrderedCache<TValue>(Configuration.CacheConfiguration,
                                            Configuration.IncludeL1Caching ? BuildL1DataStore(serviceProvider) : null,
                                            BuildL2DataStore(serviceProvider),
                                            BuildMetadata(serviceProvider),
                                            serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        /// <summary>
        /// Builds the L1 (fast lookup) data store when L1 caching is enabled.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>An <see cref="IStore{TValue}"/> for L1 caching.</returns>
        protected abstract IStore<TValue> BuildL1DataStore(IServiceProvider serviceProvider);

        /// <summary>
        /// Builds the L2 (primary) data store.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>An <see cref="IStore{TValue}"/> for L2 storage.</returns>
        protected abstract IStore<TValue> BuildL2DataStore(IServiceProvider serviceProvider);

        /// <summary>
        /// Builds the metadata store for tracking cache entries.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <returns>An <see cref="IMetadata"/> instance.</returns>
        protected abstract IMetadata BuildMetadata(IServiceProvider serviceProvider);
    }
}
