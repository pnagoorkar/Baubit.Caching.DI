using Baubit.DI;
using FluentResults;
using ByteArrayCacheModule = Baubit.Caching.DI.InMemory.Long.Module<byte[]>;
using ByteArrayCacheModuleConfiguration = Baubit.Caching.DI.InMemory.Configuration;
using GrpcServerModule = gRPC.Server.DI.Module;
using GrpcServerModuleConfiguration = gRPC.Server.DI.Configuration;

namespace ServerApp
{
    public class DevComponent : Component
    {
        protected override Result<ComponentBuilder> Build(ComponentBuilder componentBuilder)
        {
            return componentBuilder.WithModule<ByteArrayCacheModule, ByteArrayCacheModuleConfiguration>(ConfigureInMemoryCacheModule, cfg => new ByteArrayCacheModule(cfg))
                                   .WithModule<GrpcServerModule, GrpcServerModuleConfiguration>(ConfigureGrpcServerModule, cfg => new GrpcServerModule(cfg));
        }

        private void ConfigureInMemoryCacheModule(ByteArrayCacheModuleConfiguration cfg)
        {
        }

        private void ConfigureGrpcServerModule(GrpcServerModuleConfiguration cfg)
        {

        }
    }
}
