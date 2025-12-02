using Baubit.Caching.InMemory;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI.InMemory
{
    public class Module<TValue> : AModule<TValue, Configuration>
    {
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        protected override IStore<TValue> BuildL1DataStore(IServiceProvider serviceProvider)
        {
            return new Store<TValue>(Configuration.L1MinCap, Configuration.L1MaxCap, serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        protected override IStore<TValue> BuildL2DataStore(IServiceProvider serviceProvider)
        {
            return new Store<TValue>(serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        protected override IMetadata BuildMetadata(IServiceProvider serviceProvider)
        {
            return new Metadata();
        }
    }
}
