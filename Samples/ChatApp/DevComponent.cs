
using Baubit.DI;
using FluentResults;
using GrpcClientModule = gRPC.Client.DI.Module<ChatApp.ChatMessage>;
using GrpcClientModuleConfiguration = gRPC.Client.DI.Configuration;

namespace ChatApp
{
    public class DevComponent : Component
    {
        protected override Result<ComponentBuilder> Build(ComponentBuilder componentBuilder)
        {
            return componentBuilder.WithModule<GrpcClientModule, GrpcClientModuleConfiguration>(ConfigureGrpcClientModule, cfg => new GrpcClientModule(cfg));
        }

        private void ConfigureGrpcClientModule(GrpcClientModuleConfiguration cfg)
        {

        }
    }
}
