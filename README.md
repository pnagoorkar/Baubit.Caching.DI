# Baubit.Caching.DI

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI/)
[![NuGet](https://img.shields.io/nuget/dt/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI) <br/>
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)<br/>
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI)

Dependency injection modules for [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching). Registers `IOrderedCache<TId, TValue>` in your DI container with configurable L1/L2 caching and service lifetimes.

> **Important:** When using configuration-based module loading with concrete (non-generic) modules, you **MUST** call `YourModuleRegistry.Register()` before `UseConfiguredServiceProviderFactory()`. See [Configuration-Based Loading](#configuration-based-loading-extended-module) for details.

## Installation

```bash
dotnet add package Baubit.Caching.DI
```

## Quick Start

### Programmatic Module Loading

Load caching modules programmatically using `IComponent`. This is the recommended approach for generic cache modules.

```csharp
using Baubit.Caching.DI;
using Baubit.DI;
using Baubit.DI.Extensions;
using Microsoft.Extensions.DependencyInjection;

public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<InMemory.Guid7.Module<string>, InMemory.Configuration>(config =>
        {
            config.IncludeL1Caching = true;
            config.L1MinCap = 128;
            config.L1MaxCap = 8192;
            config.CacheLifetime = ServiceLifetime.Singleton;
        }, config => new InMemory.Guid7.Module<string>(config));
    }
}

await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

### Configuration-Based Loading

Create a concrete module by extending one of the generic modules and decorating it with `[BaubitModule]`.

#### Step 1: Create a Concrete Module

```csharp
using Baubit.Caching.DI;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace MyApp.Caching
{
    [BaubitModule("string-cache")]
    public class StringCacheModule : InMemory.Guid7.Module<string>
    {
        public StringCacheModule(IConfiguration configuration) : base(configuration) { }
        
        public StringCacheModule(InMemory.Configuration configuration, List<IModule> nestedModules = null) 
            : base(configuration, nestedModules) { }
    }
}
```

#### Step 2: Create Module Registry

```csharp
using Baubit.DI;

namespace MyApp.Caching
{
    [GeneratedModuleRegistry]
    internal static partial class CachingModuleRegistry
    {
        // Register() method will be generated automatically
    }
}
```

#### Step 3: Register and Load

> **CRITICAL:** You **MUST** call `Register()` on your module registry before any module loading operations. Forgetting this step will cause your modules to not be found and can lead to frustrating runtime errors.

```csharp
using MyApp.Caching;

// MUST be called before UseConfiguredServiceProviderFactory()
CachingModuleRegistry.Register();

await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build()
          .RunAsync();
```

#### Step 4: Configure in appsettings.json

```json
{
  "modules": [
    {
      "key": "string-cache",
      "configuration": {
        "includeL1Caching": true,
        "l1MinCap": 128,
        "l1MaxCap": 8192,
        "cacheLifetime": "Singleton"
      }
    }
  ]
}
```

### Hybrid Loading

Combine with other modules from appsettings.json:

```csharp
using MyApp.Caching;

// MUST call Register() first
CachingModuleRegistry.Register();

await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

## Keyed Service Registration

Register multiple cache instances with different keys for different use cases.

```csharp
using Baubit.Caching.DI;
using Baubit.DI;
using Microsoft.Extensions.DependencyInjection;

public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<InMemory.Module<long, string>, InMemory.Configuration>(config =>
                      {
                          config.RegistrationKey = "user-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new InMemory.Module<long, string>(config))
                      .WithModule<InMemory.Module<long, string>, InMemory.Configuration>(config =>
                      {
                          config.RegistrationKey = "product-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new InMemory.Module<long, string>(config));
    }
}

// Resolve keyed services
var userCache = serviceProvider.GetKeyedService<IOrderedCache<long, string>>("user-cache");
var productCache = serviceProvider.GetKeyedService<IOrderedCache<long, string>>("product-cache");
```

## Features

- **L1/L2 Caching**: Optional bounded L1 layer with unbounded L2 storage
- **Configurable Lifetimes**: Singleton, Transient, or Scoped registration
- **Keyed Service Registration**: Register multiple cache instances with unique keys
- **Type-Safe Configuration**: Strongly-typed configuration via `IComponent`
- **Sequential ID Generation**: Long and GUID v7 ID generators included

## Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncludeL1Caching` | `bool` | `false` | Enable bounded L1 caching layer |
| `L1MinCap` | `int` | `128` | Minimum capacity for L1 store |
| `L1MaxCap` | `int` | `8192` | Maximum capacity for L1 store |
| `CacheConfiguration` | `Baubit.Caching.Configuration` | `null` | Underlying cache configuration |
| `CacheLifetime` | `ServiceLifetime` | `Singleton` | DI service lifetime (Singleton, Scoped, or Transient) |
| `RegistrationKey` | `string` | `null` | Key for keyed service registration. When null, registered as non-keyed service |

## Available Modules

### `InMemory.Long.Module<TValue>`

In-memory cache module with sequential `long` ID generation starting from 1.

### `InMemory.Guid7.Module<TValue>`

In-memory cache module with monotonically increasing GUID v7 ID generation.

## Keyed Service Registration

Register multiple cache instances with different keys:

```csharp
public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<InMemory.Guid7.Module<string>, InMemory.Configuration>(config =>
                      {
                          config.RegistrationKey = "user-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new InMemory.Guid7.Module<string>(config))
                      .WithModule<InMemory.Guid7.Module<string>, InMemory.Configuration>(config =>
                      {
                          config.RegistrationKey = "product-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new InMemory.Guid7.Module<string>(config));
    }
}

// Resolve keyed services
var userCache = serviceProvider.GetKeyedService<IOrderedCache<Guid, string>>("user-cache");
var productCache = serviceProvider.GetKeyedService<IOrderedCache<Guid, string>>("product-cache");
```

## Creating Custom Modules

Extend `Module<TId, TValue, TConfiguration>` to create custom cache modules with different storage backends.

### Example: Custom Module

```csharp
using Baubit.Caching;
using Baubit.Caching.DI;
using Baubit.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace MyApp.Caching
{
    public class RedisConfiguration : Configuration
    {
        public string ConnectionString { get; set; }
        public int DatabaseNumber { get; set; } = 0;
    }

    public abstract class RedisModule<TId, TValue> : Module<TId, TValue, RedisConfiguration> 
        where TId : struct, IComparable<TId>, IEquatable<TId>
    {
        protected RedisModule(RedisConfiguration configuration, List<IModule> nestedModules = null) 
            : base(configuration, nestedModules) { }

        protected override IStore<TId, TValue> BuildL1DataStore(IServiceProvider serviceProvider)
        {
            // Implement Redis-backed L1 store
            throw new NotImplementedException();
        }

        protected override IStore<TId, TValue> BuildL2DataStore(IServiceProvider serviceProvider)
        {
            // Implement Redis-backed L2 store
            throw new NotImplementedException();
        }

        protected override IMetadata<TId> BuildMetadata(IServiceProvider serviceProvider)
        {
            // Implement Redis-backed metadata store
            throw new NotImplementedException();
        }
    }
}
```

## Dependencies

- [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching) - Core caching abstractions
- [Baubit.DI.Extensions](https://github.com/pnagoorkar/Baubit.DI.Extensions) - Dependency injection modularity framework

## License

[MIT](LICENSE)
