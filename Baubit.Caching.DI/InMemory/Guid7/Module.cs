using Baubit.DI;
using Baubit.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI.InMemory.Guid7
{
    public class Module<TValue> : InMemory.Module<Guid, TValue>
    {
        IIdentityGenerator identityGenerator = IdentityGenerator.CreateNew();
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

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
