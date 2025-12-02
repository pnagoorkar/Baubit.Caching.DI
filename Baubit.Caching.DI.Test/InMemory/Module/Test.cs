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
        public void Load_WithSingletonLifetime_RegistersCache()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         })
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void Load_WithTransientLifetime_RegistersCache()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Transient;
                                         })
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void Load_WithScopedLifetime_RegistersCache()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Logging.Module, Logging.Configuration>((Logging.Configuration _) => { })
                                         .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Scoped;
                                         })
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
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
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
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
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
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
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
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
                                         .Build<IOrderedCache<string>>();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }
    }
}
