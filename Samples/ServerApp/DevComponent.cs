using Baubit.DI;
using FluentResults;
using ByteArrayCacheModule = Baubit.Caching.DI.InMemory.Long.Module<byte[]>;
using ByteArrayCacheModuleConfiguration = Baubit.Caching.DI.InMemory.Configuration;
using GrpcServerModule = gRPC.Server.DI.Module;
using GrpcServerModuleConfiguration = gRPC.Server.DI.Configuration;

namespace ServerApp
{
    /// <summary>
    /// Dependency injection configuration component for the chat server.
    /// Registers the in-memory cache module and gRPC server module for distributed caching.
    /// </summary>
    public class DevComponent : Component
    {
        /// <summary>
        /// Builds the component by registering required modules:
        /// - In-memory cache module with long IDs for storing serialized messages
        /// - gRPC server module for exposing cache operations via gRPC
        /// </summary>
        /// <param name="componentBuilder">The component builder to configure.</param>
        /// <returns>A result containing the configured component builder.</returns>
        protected override Result<ComponentBuilder> Build(ComponentBuilder componentBuilder)
        {
            return componentBuilder.WithModule<ByteArrayCacheModule, ByteArrayCacheModuleConfiguration>(ConfigureInMemoryCacheModule, cfg => new ByteArrayCacheModule(cfg))
                                   .WithModule<GrpcServerModule, GrpcServerModuleConfiguration>(ConfigureGrpcServerModule, cfg => new GrpcServerModule(cfg));
        }

        /// <summary>
        /// Configures the in-memory cache module.
        /// The cache uses byte arrays to store MessagePack-serialized chat messages.
        /// </summary>
        /// <param name="cfg">The configuration object to customize.</param>
        private void ConfigureInMemoryCacheModule(ByteArrayCacheModuleConfiguration cfg)
        {
            // Default configuration is used
            // To enable L1 caching: cfg.IncludeL1Caching = true;
        }

        /// <summary>
        /// Configures the gRPC server module that exposes the cache via gRPC endpoints.
        /// </summary>
        /// <param name="cfg">The configuration object to customize.</param>
        private void ConfigureGrpcServerModule(GrpcServerModuleConfiguration cfg)
        {
            // Default configuration is used
        }
    }
}
