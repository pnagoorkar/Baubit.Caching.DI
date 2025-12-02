using Baubit.DI;
using Baubit.DI.Extensions;

namespace Baubit.Caching.DI.Test.InMemory.Module
{
    /// <summary>
    /// Unit tests for <see cref="DI.InMemory.Module{TValue}"/>
    /// </summary>
    public class Test
    {
        public class TestContext
        {
            public IOrderedCache<string> OrderedCache { get; init; }
            public TestContext(IOrderedCache<string> orderedCache)
            {
                OrderedCache = orderedCache;
            }
        }
        // rename the test method appropriately
        public void Works()
        {
            var componentBuildResult = ComponentBuilder.CreateNew()
                                                       .WithModule<DI.InMemory.Module<string>, DI.InMemory.Configuration>(BuildConfiguration)
                                                       .Build()
                                                       .Build<TestContext>();

            Assert.True(componentBuildResult.IsSuccess);
            Assert.NotNull(componentBuildResult.Value);
            Assert.NotNull(componentBuildResult.Value.OrderedCache);
        }

        private void BuildConfiguration(DI.InMemory.Configuration configuration)
        {

        }
    }
}
