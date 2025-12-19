# Baubit.Caching.DI

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI/)
[![NuGet](https://img.shields.io/nuget/dt/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI) <br/>
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)<br/>
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI)

Dependency injection modules for [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching). Registers `IOrderedCache<TValue>` in your DI container with configurable L1/L2 caching and service lifetimes.

## Installation

```bash
dotnet add package Baubit.Caching.DI
```

## Quick Start

### Pattern 1: Modules from appsettings.json

Load modules from configuration. Module types and settings are defined in JSON.

```csharp
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build()
          .RunAsync();
```

**appsettings.json:**
```json
{
  "type": "Baubit.Caching.DI.InMemory.Module`1[[System.String]], Baubit.Caching.DI",
  "configuration": {
    "includeL1Caching": true,
    "l1MinCap": 128,
    "l1MaxCap": 8192,
    "cacheLifetime": "Singleton",
    "registrationKey": "my-cache",
    "modules": [
      {
        "type": "MyApp.LoggingModule, MyApp", // Register ILoggerFactory for OrderedCache<T>
        "configuration": {}
      }
    ]
  }
}
```

### Pattern 2: Modules from Code (IComponent)

Load modules programmatically using `IComponent`.

```csharp
public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<LoggingModule, LoggingConfiguration>(
                          ConfigureLogging, 
                          config => new LoggingModule(config)) // Register ILoggerFactory for OrderedCache<T>
                      .WithModule<Module<string>, Configuration>(
                          ConfigureCaching, 
                          config => new Module<string>(config));
    }
    private void ConfigureLogging(LoggingConfiguration config) 
    {
        // configure as needed
    }
    private void ConfigureCaching(Configuration config) 
    {
        config.IncludeL1Caching = true;
        config.CacheLifetime = ServiceLifetime.Singleton;
    }
}

await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

### Pattern 3: Hybrid Loading

Combine configuration-based and code-based module loading.

```csharp
await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

## Keyed Service Registration

It is also possible to register multiple cache instances (of the same type) with different keys.

```csharp
public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<LoggingModule, LoggingConfiguration>(
                          ConfigureLogging, 
                          config => new LoggingModule(config))
                      .WithModule<Module<string>, Configuration>(config =>
                      {
                          config.RegistrationKey = "user-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new Module<string>(config))
                      .WithModule<Module<string>, Configuration>(config =>
                      {
                          config.RegistrationKey = "product-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new Module<string>(config));
    }
}

// Resolve keyed services
var userCache = serviceProvider.GetKeyedService<IOrderedCache<string>>("user-cache");
var productCache = serviceProvider.GetKeyedService<IOrderedCache<string>>("product-cache");
```

## Features

- **L1/L2 Caching**: Optional bounded L1 (fast lookup) layer with unbounded L2 storage
- **Configurable Lifetimes**: Singleton, Transient, or Scoped registration
- **Keyed Service Registration**: Register caches with a key for multi-instance scenarios
- **IConfiguration Support**: Load settings from appsettings.json or other configuration sources

## Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncludeL1Caching` | `bool` | `false` | Enable bounded L1 caching layer |
| `L1MinCap` | `int` | `128` | Minimum capacity for L1 store |
| `L1MaxCap` | `int` | `8192` | Maximum capacity for L1 store |
| `CacheConfiguration` | `Configuration` | `null` | Underlying cache configuration |
| `CacheLifetime` | `ServiceLifetime` | `Singleton` | DI service lifetime |
| `RegistrationKey` | `string` | `null` | Key for keyed service registration |

## API Reference

### `Configuration`

Abstract base configuration class for caching modules.

### `Module<TValue, TConfiguration>`

Abstract base module for registering `IOrderedCache<TValue>`. Implement `BuildL1DataStore`, `BuildL2DataStore`, and `BuildMetadata` to customize cache construction.

### `InMemory.Module<TValue>`

Concrete module using in-memory stores. L1 uses bounded `Store<TValue>` with capacity limits. L2 uses unbounded `Store<TValue>`.

## Dependencies

- [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching)
- [Baubit.DI.Extensions](https://github.com/pnagoorkar/Baubit.DI.Extensions)

## License

[MIT](LICENSE)
