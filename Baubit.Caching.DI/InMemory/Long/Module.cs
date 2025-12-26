using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Baubit.Caching.DI.InMemory.Long
{
    public class Module<TValue> : InMemory.Module<long, TValue>
    {
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        protected override long? GenerateNextId(long? id)
        {
            return id == null ? 1 : id + 1;
        }
    }
}
