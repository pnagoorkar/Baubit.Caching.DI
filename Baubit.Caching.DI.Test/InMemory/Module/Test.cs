using Baubit.DI;
using Baubit.DI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Baubit.Caching.DI.Test.InMemory.Module
{
    /// <summary>
    /// Unit tests for <see cref="DI.InMemory.Module{TValue}"/>
    /// </summary>
    public class Test
    {
        private static IServiceProvider BuildServiceProvider(Action<DI.InMemory.Configuration> configureAction)
        {
            var componentResult = ComponentBuilder.CreateNew()
                                                  .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(configureAction);
            Assert.True(componentResult.IsSuccess);

            var buildResult = componentResult.Value.Build();
            Assert.True(buildResult.IsSuccess);

            var services = new ServiceCollection();
            services.AddLogging();
            foreach (var m in buildResult.Value)
            {
                m.Load(services);
            }

            return services.BuildServiceProvider();
        }

        [Fact]
        public void Load_WithSingletonLifetime_RegistersCache()
        {
            var serviceProvider = BuildServiceProvider(config =>
            {
                config.CacheLifetime = ServiceLifetime.Singleton;
            });

            var cache = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithTransientLifetime_RegistersCache()
        {
            var serviceProvider = BuildServiceProvider(config =>
            {
                config.CacheLifetime = ServiceLifetime.Transient;
            });

            var cache = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithScopedLifetime_RegistersCache()
        {
            var serviceProvider = BuildServiceProvider(config =>
            {
                config.CacheLifetime = ServiceLifetime.Scoped;
            });

            using var scope = serviceProvider.CreateScope();
            var cache = scope.ServiceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithL1CachingEnabled_RegistersCacheWithL1Store()
        {
            var serviceProvider = BuildServiceProvider(config =>
            {
                config.IncludeL1Caching = true;
                config.L1MinCap = 64;
                config.L1MaxCap = 1024;
            });

            var cache = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithL1CachingDisabled_RegistersCacheWithoutL1Store()
        {
            var serviceProvider = BuildServiceProvider(config =>
            {
                config.IncludeL1Caching = false;
            });

            var cache = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithCacheConfiguration_RegistersCacheWithConfiguration()
        {
            var serviceProvider = BuildServiceProvider(config =>
            {
                config.CacheConfiguration = new Baubit.Caching.Configuration();
            });

            var cache = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }

        [Fact]
        public void Constructor_WithConfiguration_CreatesModule()
        {
            var config = new DI.InMemory.Configuration();
            var module = new DI.InMemory.Module<string>(config);

            Assert.NotNull(module);
        }

        [Fact]
        public void Constructor_WithConfigurationAndNestedModules_CreatesModule()
        {
            var config = new DI.InMemory.Configuration();
            var nestedModules = new System.Collections.Generic.List<IModule>();
            var module = new DI.InMemory.Module<string>(config, nestedModules);

            Assert.NotNull(module);
        }

        [Fact]
        public void Constructor_WithIConfiguration_CreatesModule()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IncludeL1Caching"] = "true",
                ["L1MinCap"] = "64",
                ["L1MaxCap"] = "1024",
                ["CacheLifetime"] = "Singleton"
            });
            var configuration = configBuilder.Build();

            var module = new DI.InMemory.Module<string>(configuration);

            Assert.NotNull(module);

            var services = new ServiceCollection();
            services.AddLogging();
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache);
        }
    }
}
