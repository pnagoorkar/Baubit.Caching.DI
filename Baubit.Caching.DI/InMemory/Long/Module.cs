using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Baubit.Caching.DI.InMemory.Long
{
    /// <summary>
    /// Dependency injection module for registering an in-memory ordered cache with <see cref="long"/> IDs.
    /// Generates sequential long IDs starting from 1.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
    public class Module<TValue> : InMemory.Module<long, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Module{TValue}"/> class
        /// using an <see cref="IConfiguration"/> to bind settings.
        /// </summary>
        /// <param name="configuration">The configuration section to bind to <see cref="Configuration"/>.</param>
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Module{TValue}"/> class
        /// using an explicit configuration object and optional nested modules.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        /// <param name="nestedModules">Optional list of nested modules to load.</param>
        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        /// <summary>
        /// Generates the next sequential long ID.
        /// </summary>
        /// <param name="id">The last generated ID, or <c>null</c> to start from 1.</param>
        /// <returns>The next ID in sequence (starting from 1).</returns>
        protected override long? GenerateNextId(long? id)
        {
            if (id == null)
            {
                return 1;
            }
            return id + 1;
        }
    }
}
