using Baubit.DI;
using Baubit.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI.InMemory.Guid7
{
    /// <summary>
    /// Dependency injection module for registering an in-memory ordered cache with GUID v7 IDs.
    /// Generates monotonically increasing GUIDs using <see cref="IIdentityGenerator"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
    public class Module<TValue> : InMemory.Module<Guid, TValue>
    {
        IIdentityGenerator identityGenerator = IdentityGenerator.CreateNew();

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
        /// Generates the next GUID v7 in monotonically increasing sequence.
        /// </summary>
        /// <param name="lastGeneratedId">The last generated GUID, used to ensure monotonicity.</param>
        /// <returns>The next GUID v7 in sequence, or <c>null</c> if generation fails.</returns>
        protected override Guid? GenerateNextId(Guid? lastGeneratedId)
        {
            if (identityGenerator == null) return null;
            // Initialize from last generated ID if available to ensure monotonicity
            if (lastGeneratedId.HasValue)
            {
                identityGenerator.InitializeFrom(lastGeneratedId.Value);
            }

            return identityGenerator.GetNext();
        }
    }
}
