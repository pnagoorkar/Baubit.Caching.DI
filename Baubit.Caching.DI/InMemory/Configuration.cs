namespace Baubit.Caching.DI.InMemory
{
    /// <summary>
    /// Configuration for the in-memory caching module.
    /// Uses <see cref="Baubit.Caching.InMemory.Store{TValue}"/> for both L1 and L2 data stores.
    /// </summary>
    public class Configuration : DI.Configuration
    {
    }
}
