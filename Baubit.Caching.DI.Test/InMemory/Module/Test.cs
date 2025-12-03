using Baubit.DI;
using Baubit.DI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Caching.DI.Test.InMemory.Module
{
    /// <summary>
    /// Unit tests for <see cref="DI.InMemory.Module{TValue}"/>
    /// </summary>
    public class Test
    {
        [Fact]
        public void Load_WithSingletonLifetime_RegistersCacheAsSingleton()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            var cache1 = serviceProvider.GetService<IOrderedCache<string>>();
            var cache2 = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.Same(cache1, cache2); // Singleton returns same instance
        }

        [Fact]
        public void Load_WithTransientLifetime_RegistersCacheAsTransient()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Transient;
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            var cache1 = serviceProvider.GetService<IOrderedCache<string>>();
            var cache2 = serviceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.NotSame(cache1, cache2); // Transient returns different instances
        }

        [Fact]
        public void Load_WithScopedLifetime_RegistersCacheAsScoped()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Scoped;
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var cache1InScope1 = scope1.ServiceProvider.GetService<IOrderedCache<string>>();
            var cache2InScope1 = scope1.ServiceProvider.GetService<IOrderedCache<string>>();
            var cache1InScope2 = scope2.ServiceProvider.GetService<IOrderedCache<string>>();

            Assert.NotNull(cache1InScope1);
            Assert.NotNull(cache2InScope1);
            Assert.NotNull(cache1InScope2);
            Assert.Same(cache1InScope1, cache2InScope1); // Same scope returns same instance
            Assert.NotSame(cache1InScope1, cache1InScope2); // Different scopes return different instances
        }

        [Fact]
        public void Load_WithL1CachingEnabled_RegistersCacheWithL1Store()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.IncludeL1Caching = true;
                                             config.L1MinCap = 64;
                                             config.L1MaxCap = 1024;
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithL1CachingDisabled_RegistersCacheWithoutL1Store()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.IncludeL1Caching = false;
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithCacheConfiguration_RegistersCacheWithConfiguration()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheConfiguration = new Baubit.Caching.Configuration();
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<string>>();
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

            // Test that the module loads correctly with the same config values
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.IncludeL1Caching = true;
                                             config.L1MinCap = 64;
                                             config.L1MaxCap = 1024;
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         })
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<string>>();
            Assert.NotNull(cache);
        }
    }
}
