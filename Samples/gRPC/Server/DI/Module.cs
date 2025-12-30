using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace gRPC.Server.DI
{
    public class Module : Baubit.DI.Module<Configuration>
    {
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        public override void Load(IServiceCollection services)
        {
            var options = MessagePack.Resolvers.ContractlessStandardResolver.Options;
            var serializer = new Serializer(options);
            services.AddSingleton<ISerializer>(serializer);
        }
    }
}
