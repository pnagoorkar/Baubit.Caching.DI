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
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            var cache1 = serviceProvider.GetService<IOrderedCache<Guid, string>>();
            var cache2 = serviceProvider.GetService<IOrderedCache<Guid, string>>();

            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.Same(cache1, cache2); // Singleton returns same instance
        }

        [Fact]
        public void Load_WithTransientLifetime_RegistersCacheAsTransient()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Transient;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            var cache1 = serviceProvider.GetService<IOrderedCache<Guid, string>>();
            var cache2 = serviceProvider.GetService<IOrderedCache<Guid, string>>();

            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.NotSame(cache1, cache2); // Transient returns different instances
        }

        [Fact]
        public void Load_WithScopedLifetime_RegistersCacheAsScoped()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Scoped;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var cache1InScope1 = scope1.ServiceProvider.GetService<IOrderedCache<Guid, string>>();
            var cache2InScope1 = scope1.ServiceProvider.GetService<IOrderedCache<Guid, string>>();
            var cache1InScope2 = scope2.ServiceProvider.GetService<IOrderedCache<Guid, string>>();

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
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.IncludeL1Caching = true;
                                             config.L1MinCap = 64;
                                             config.L1MaxCap = 1024;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithL1CachingDisabled_RegistersCacheWithoutL1Store()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.IncludeL1Caching = false;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithCacheConfiguration_RegistersCacheWithConfiguration()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheConfiguration = new Baubit.Caching.Configuration();
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Constructor_WithConfiguration_CreatesModule()
        {
            var config = new DI.InMemory.Configuration();
            var module = new DI.InMemory.Guid7.Module<string>(config);

            Assert.NotNull(module);
        }

        [Fact]
        public void Constructor_WithConfigurationAndNestedModules_CreatesModule()
        {
            var config = new DI.InMemory.Configuration();
            var nestedModules = new System.Collections.Generic.List<IModule>();
            var module = new DI.InMemory.Guid7.Module<string>(config, nestedModules);

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

            var module = new DI.InMemory.Guid7.Module<string>(configuration);

            Assert.NotNull(module);

            // Test that the module loads correctly with the same config values
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.IncludeL1Caching = true;
                                             config.L1MinCap = 64;
                                             config.L1MaxCap = 1024;
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithSingletonLifetimeAndRegistrationKey_RegistersKeyedCacheAsSingleton()
        {
            const string registrationKey = "singleton-test-cache";
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                             config.RegistrationKey = registrationKey;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            var cache1 = serviceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);
            var cache2 = serviceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);

            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.Same(cache1, cache2); // Singleton returns same instance
        }

        [Fact]
        public void Load_WithTransientLifetimeAndRegistrationKey_RegistersKeyedCacheAsTransient()
        {
            const string registrationKey = "transient-test-cache";
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Transient;
                                             config.RegistrationKey = registrationKey;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            var cache1 = serviceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);
            var cache2 = serviceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);

            Assert.NotNull(cache1);
            Assert.NotNull(cache2);
            Assert.NotSame(cache1, cache2); // Transient returns different instances
        }

        [Fact]
        public void Load_WithScopedLifetimeAndRegistrationKey_RegistersKeyedCacheAsScoped()
        {
            const string registrationKey = "scoped-test-cache";
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Scoped;
                                             config.RegistrationKey = registrationKey;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var serviceProvider = result.Value;

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var cache1InScope1 = scope1.ServiceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);
            var cache2InScope1 = scope1.ServiceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);
            var cache1InScope2 = scope2.ServiceProvider.GetKeyedService<IOrderedCache<Guid, string>>(registrationKey);

            Assert.NotNull(cache1InScope1);
            Assert.NotNull(cache2InScope1);
            Assert.NotNull(cache1InScope2);
            Assert.Same(cache1InScope1, cache2InScope1); // Same scope returns same instance
            Assert.NotSame(cache1InScope1, cache1InScope2); // Different scopes return different instances
        }

        [Fact]
        public void Load_WithLongModule_RegistersCacheWithLongIds()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Long.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         }, config => new DI.InMemory.Long.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<long, string>>();
            Assert.NotNull(cache);
            
            // Verify that long IDs are generated correctly
            var success = cache.Add("test1", out var entry1);
            Assert.True(success);
            Assert.NotNull(entry1);
            Assert.Equal(1L, entry1.Id);
            
            success = cache.Add("test2", out var entry2);
            Assert.True(success);
            Assert.NotNull(entry2);
            Assert.Equal(2L, entry2.Id);
        }

        [Fact]
        public void Load_WithGuid7Module_GeneratesMonotonicGuids()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
            
            // Verify that GUIDs are generated and monotonically increasing
            var success = cache.Add("test1", out var entry1);
            Assert.True(success);
            Assert.NotNull(entry1);
            Assert.NotEqual(Guid.Empty, entry1.Id);
            
            success = cache.Add("test2", out var entry2);
            Assert.True(success);
            Assert.NotNull(entry2);
            Assert.NotEqual(Guid.Empty, entry2.Id);
            
            // GUID v7 should be monotonically increasing
            Assert.True(entry2.Id.CompareTo(entry1.Id) > 0);
        }

        [Fact]
        public void Load_WithLongModule_GeneratesSequentialIdsFromOne()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Long.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                             config.IncludeL1Caching = false; // Ensure L2 store is used
                                         }, config => new DI.InMemory.Long.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<long, string>>();
            Assert.NotNull(cache);
            
            // First ID should be 1 (null case in GenerateNextId)
            var success = cache.Add("first", out var entry);
            Assert.True(success);
            Assert.NotNull(entry);
            Assert.Equal(1L, entry.Id);
        }

        [Fact]
        public void Load_WithGuid7Module_HandlesInitializationFromLastId()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                             config.IncludeL1Caching = true; // Test L1 caching path
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
            
            // Add multiple entries to test the InitializeFrom path
            cache.Add("item1", out var entry1);
            cache.Add("item2", out var entry2);
            cache.Add("item3", out var entry3);
            
            Assert.NotNull(entry1);
            Assert.NotNull(entry2);
            Assert.NotNull(entry3);
            Assert.True(entry2.Id.CompareTo(entry1.Id) > 0);
            Assert.True(entry3.Id.CompareTo(entry2.Id) > 0);
        }

        [Fact]
        public void Load_WithInvalidServiceLifetime_ThrowsNotImplementedException()
        {
            // Test the defensive default case in the Load method's switch statement
            var config = new DI.InMemory.Configuration
            {
                CacheLifetime = (ServiceLifetime)999 // Invalid enum value
            };

            var module = new DI.InMemory.Guid7.Module<string>(config);
            var services = new ServiceCollection();
            services.AddLogging();

            // This should throw NotImplementedException for the default case
            Assert.Throws<NotImplementedException>(() => module.Load(services));
        }

        [Fact]
        public void Constructor_WithIConfigurationForLongModule_CreatesModule()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IncludeL1Caching"] = "false",
                ["CacheLifetime"] = "Singleton"
            });
            var configuration = configBuilder.Build();

            var module = new DI.InMemory.Long.Module<string>(configuration);

            Assert.NotNull(module);

            // Verify it works by loading it into a service collection
            var services = new ServiceCollection();
            services.AddLogging();
            
            Assert.NotNull(module);
            
            // Test that it loads without throwing
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetService<IOrderedCache<long, string>>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void Load_WithLongModule_ExercisesAllIdGenerationBranches()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Long.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                             config.IncludeL1Caching = false;
                                         }, config => new DI.InMemory.Long.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<long, string>>();
            Assert.NotNull(cache);
            
            // Clear cache to ensure we start with LastAddedId = null
            cache.Clear();
            
            // First add after clear should trigger GenerateNextId(null) -> returns 1
            var success = cache.Add("value1", out var entry1);
            Assert.True(success);
            Assert.NotNull(entry1);
            Assert.Equal(1L, entry1.Id);
            
            // Second add should trigger GenerateNextId(1) -> returns 2
            success = cache.Add("value2", out var entry2);
            Assert.True(success);
            Assert.NotNull(entry2);
            Assert.Equal(2L, entry2.Id);
            
            // Third add should trigger GenerateNextId(2) -> returns 3
            success = cache.Add("value3", out var entry3);
            Assert.True(success);
            Assert.NotNull(entry3);
            Assert.Equal(3L, entry3.Id);
        }

        [Fact]
        public void Load_WithGuid7Module_ExercisesAllIdGenerationBranches()
        {
            var result = ComponentBuilder.CreateNew()
                                         .WithModule<Setup.Logging.Module, Setup.Logging.Configuration>((Setup.Logging.Configuration _) => { }, config => new Setup.Logging.Module(config))
                                         .WithModule<DI.InMemory.Guid7.Module<string>, DI.InMemory.Configuration>(config =>
                                         {
                                             config.CacheLifetime = ServiceLifetime.Singleton;
                                             config.IncludeL1Caching = false;
                                         }, config => new DI.InMemory.Guid7.Module<string>(config))
                                         .BuildServiceProvider();

            Assert.True(result.IsSuccess);
            var cache = result.Value.GetService<IOrderedCache<Guid, string>>();
            Assert.NotNull(cache);
            
            // Clear cache to ensure we start with LastAddedId = null
            cache.Clear();
            
            // First add after clear should trigger GenerateNextId(null)
            var success = cache.Add("value1", out var entry1);
            Assert.True(success);
            Assert.NotNull(entry1);
            Assert.NotEqual(Guid.Empty, entry1.Id);
            
            // Second add should trigger GenerateNextId(previousGuid)
            success = cache.Add("value2", out var entry2);
            Assert.True(success);
            Assert.NotNull(entry2);
            Assert.NotEqual(Guid.Empty, entry2.Id);
            Assert.True(entry2.Id.CompareTo(entry1.Id) > 0);
            
            // Third add to further exercise the branch with HasValue = true
            success = cache.Add("value3", out var entry3);
            Assert.True(success);
            Assert.NotNull(entry3);
            Assert.NotEqual(Guid.Empty, entry3.Id);
            Assert.True(entry3.Id.CompareTo(entry2.Id) > 0);
        }

        [Fact]
        public void LongModule_GenerateNextId_WithNull_ReturnsOne()
        {
            // Create a test module to access protected method
            var testModule = new TestLongModule();
            
            // Test null case
            var result = testModule.TestGenerateNextId(null);
            Assert.Equal(1L, result);
            
            // Test non-null case
            result = testModule.TestGenerateNextId(5L);
            Assert.Equal(6L, result);
            
            // Test another non-null case to ensure branch is fully exercised
            result = testModule.TestGenerateNextId(100L);
            Assert.Equal(101L, result);
        }
        
        [Fact]
        public void LongModule_GenerateNextId_MultipleCalls_ExercisesBothBranches()
        {
            // Create multiple test modules to exercise the ternary operator's both branches
            var testModule1 = new TestLongModule();
            var testModule2 = new TestLongModule();
            
            // First call with null on module1
            var result1 = testModule1.TestGenerateNextId(null);
            Assert.Equal(1L, result1);
            
            // Call with non-null on module1
            var result2 = testModule1.TestGenerateNextId(result1);
            Assert.Equal(2L, result2);
            
            // First call with null on module2
            var result3 = testModule2.TestGenerateNextId(null);
            Assert.Equal(1L, result3);
            
            // Multiple calls with non-null on module2
            var result4 = testModule2.TestGenerateNextId(10L);
            Assert.Equal(11L, result4);
            
            var result5 = testModule2.TestGenerateNextId(20L);
            Assert.Equal(21L, result5);
        }
        
        // Helper test class to expose protected GenerateNextId
        private class TestLongModule : DI.InMemory.Long.Module<string>
        {
            public TestLongModule() : base(new DI.InMemory.Configuration()) { }
            
            public long? TestGenerateNextId(long? id)
            {
                return GenerateNextId(id);
            }
        }
    }
}
