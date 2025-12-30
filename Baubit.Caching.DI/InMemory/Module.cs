using Baubit.Caching.InMemory;
using Baubit.DI;
using Baubit.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI.InMemory
{
    /// <summary>
    /// Dependency injection module for registering an in-memory ordered cache.
    /// Uses in-memory stores for both L1 and L2 data stores.
    /// </summary>
    /// <typeparam name="TId">The type of IDs used to identify cache entries.</typeparam>
    /// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
    public abstract class Module<TId, TValue> : DI.Module<TId, TValue, Configuration> where TId : struct, IComparable<TId>, IEquatable<TId>
    {
        /// <summary>
        /// Initializes a new instance of the module class
        /// using an <see cref="IConfiguration"/> to bind settings.
        /// </summary>
        /// <param name="configuration">The configuration section to bind to <see cref="Configuration"/>.</param>
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the module class
        /// using an explicit configuration object and optional nested modules.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="nestedModules">Optional list of nested modules to load.</param>
        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        /// <summary>
        /// Builds the L1 data store as a bounded in-memory store with capacity limits.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve <see cref="ILoggerFactory"/>.</param>
        /// <returns>A bounded in-memory store configured with L1 capacity settings.</returns>
        protected override IStore<TId, TValue> BuildL1DataStore(IServiceProvider serviceProvider)
        {
            return new Caching.InMemory.Store<TId, TValue>(Configuration.L1MinCap, Configuration.L1MaxCap, GenerateNextId, serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        /// <summary>
        /// Generates the next ID in sequence for cache entries.
        /// </summary>
        /// <param name="id">The last generated ID, or <c>null</c> if no ID has been generated yet.</param>
        /// <returns>The next ID in sequence, or <c>null</c> if ID generation is not supported.</returns>
        protected abstract TId? GenerateNextId(TId? id);

        /// <summary>
        /// Builds the L2 data store as an unbounded in-memory store.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve <see cref="ILoggerFactory"/>.</param>
        /// <returns>An unbounded in-memory store.</returns>
        protected override IStore<TId, TValue> BuildL2DataStore(IServiceProvider serviceProvider)
        {
            return new Caching.InMemory.Store<TId, TValue>(Configuration.L1MinCap, Configuration.L1MaxCap, GenerateNextId, serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        /// <summary>
        /// Builds the metadata store for tracking cache entries.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve <see cref="ILoggerFactory"/>.</param>
        /// <returns>A new metadata store instance.</returns>
        protected override IMetadata<TId> BuildMetadata(IServiceProvider serviceProvider)
        {
            return new Metadata<TId>(Configuration.CacheConfiguration, serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}
