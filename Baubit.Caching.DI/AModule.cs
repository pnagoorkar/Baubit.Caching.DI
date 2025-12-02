using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Caching.DI
{
    public abstract class AModule<TValue, TConfiguration> : Baubit.DI.AModule<TConfiguration> where TConfiguration : AConfiguration
    {
        protected AModule(IConfiguration configuration) : base(configuration)
        {
        }

        protected AModule(TConfiguration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        public override void Load(IServiceCollection services)
        {
            switch (Configuration.CacheLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(BuildOrderedCache);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(BuildOrderedCache);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(BuildOrderedCache);
                    break;
                default: throw new NotImplementedException();
            }
            base.Load(services);
        }

        private IOrderedCache<TValue> BuildOrderedCache(IServiceProvider serviceProvider)
        {
            return new OrderedCache<TValue>(Configuration.CacheConfiguration,
                                            Configuration.IncludeL1Caching ? BuildL1DataStore(serviceProvider) : null,
                                            BuildL2DataStore(serviceProvider),
                                            BuildMetadata(serviceProvider),
                                            serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        protected abstract IStore<TValue> BuildL1DataStore(IServiceProvider serviceProvider);

        protected abstract IStore<TValue> BuildL2DataStore(IServiceProvider serviceProvider);

        protected abstract IMetadata BuildMetadata(IServiceProvider serviceProvider);
    }
}
