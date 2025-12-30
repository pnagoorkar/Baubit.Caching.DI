
using Baubit.DI;
using FluentResults;
using GrpcClientModule = gRPC.Client.DI.Module<ChatApp.ChatMessage>;
using GrpcClientModuleConfiguration = gRPC.Client.DI.Configuration;

namespace ChatApp
{
    /// <summary>
    /// Dependency injection configuration component for the chat application.
    /// Registers the gRPC client module that connects to the distributed cache server.
    /// </summary>
    public class DevComponent : Component
    {
        /// <summary>
        /// Builds the component by registering the gRPC client module.
        /// The client module provides an IOrderedCache implementation that communicates
        /// with the server via gRPC for distributed message synchronization.
        /// </summary>
        /// <param name="componentBuilder">The component builder to configure.</param>
        /// <returns>A result containing the configured component builder.</returns>
        protected override Result<ComponentBuilder> Build(ComponentBuilder componentBuilder)
        {
            return componentBuilder.WithModule<GrpcClientModule, GrpcClientModuleConfiguration>(ConfigureGrpcClientModule, cfg => new GrpcClientModule(cfg));
        }

        /// <summary>
        /// Configures the gRPC client module settings.
        /// Default configuration uses http://localhost:49971 as the server address.
        /// </summary>
        /// <param name="cfg">The configuration object to customize.</param>
        private void ConfigureGrpcClientModule(GrpcClientModuleConfiguration cfg)
        {
            // Default configuration is used (http://localhost:49971)
            // To customize: cfg.GrpcChannelAddress = "http://your-server:port";
        }
    }
}
