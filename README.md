# Baubit.Caching.DI

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI/)
[![NuGet](https://img.shields.io/nuget/dt/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI) <br/>
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)<br/>
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI)

Dependency injection modules for [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching). Registers `IOrderedCache<TValue>` in your DI container with configurable L1/L2 caching and service lifetimes.

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
        return builder.WithModule<InMemory.Module<string>, InMemory.Configuration>(config =>
        {
            config.IncludeL1Caching = true;
            config.L1MinCap = 128;
            config.L1MaxCap = 8192;
            config.CacheLifetime = ServiceLifetime.Singleton;
        }, config => new InMemory.Module<string>(config));
    }
}

await Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
          .UseConfiguredServiceProviderFactory(componentsFactory: () => [new AppComponent()])
          .Build()
          .RunAsync();
```

### Configuration-Based Loading (Extended Module)

Since `InMemory.Module<TValue>` is generic, it **cannot** be decorated with `[BaubitModule]` directly. To load from configuration, extend it to create a concrete, non-generic module:

#### Step 1: Create a Concrete Module

```csharp
using Baubit.Caching.DI;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MyApp.Caching
{
    /// <summary>
    /// Concrete string cache module that can be loaded from configuration.
    /// </summary>
    [BaubitModule("string-cache")]
    public class StringCacheModule : InMemory.Module<string>
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
        "cacheLifetime": "Singleton",
        "registrationKey": "my-cache"
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
        return builder.WithModule<InMemory.Module<string>, InMemory.Configuration>(config =>
                      {
                          config.RegistrationKey = "user-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new InMemory.Module<string>(config))
                      .WithModule<InMemory.Module<string>, InMemory.Configuration>(config =>
                      {
                          config.RegistrationKey = "product-cache";
                          config.CacheLifetime = ServiceLifetime.Singleton;
                      }, config => new InMemory.Module<string>(config));
    }
}

// Resolve keyed services
var userCache = serviceProvider.GetKeyedService<IOrderedCache<string>>("user-cache");
var productCache = serviceProvider.GetKeyedService<IOrderedCache<string>>("product-cache");
```

## Features

- **L1/L2 Caching**: Optional bounded L1 (fast lookup) layer with unbounded L2 storage
- **Configurable Lifetimes**: Singleton, Transient, or Scoped registration
- **Keyed Service Registration**: Register multiple cache instances with unique keys
- **Type-Safe Configuration**: Strongly-typed configuration via `IComponent`
- **Flexible Storage**: Implement custom storage backends by extending `Module<TValue, TConfiguration>`

## Configuration

Configuration properties for caching modules:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncludeL1Caching` | `bool` | `false` | Enable bounded L1 caching layer |
| `L1MinCap` | `int` | `128` | Minimum capacity for L1 store |
| `L1MaxCap` | `int` | `8192` | Maximum capacity for L1 store |
| `CacheConfiguration` | `Baubit.Caching.Configuration` | `null` | Underlying cache configuration |
| `CacheLifetime` | `ServiceLifetime` | `Singleton` | DI service lifetime (Singleton, Scoped, or Transient) |
| `RegistrationKey` | `string` | `null` | Key for keyed service registration. When null, registered as non-keyed service |

## Available Modules

### `InMemory.Module<TValue>`

Concrete module using in-memory stores for both L1 and L2 caching layers.

**Configuration:** Uses `InMemory.Configuration` which extends the base `Configuration` class.

**Storage:**
- **L1**: Bounded `Store<TValue>` with configurable capacity (`L1MinCap` to `L1MaxCap`)
- **L2**: Unbounded `Store<TValue>`
- **Metadata**: In-memory `Metadata` store

**Example:**
```csharp
builder.WithModule<InMemory.Module<string>, InMemory.Configuration>(config =>
{
    config.IncludeL1Caching = true;
    config.L1MinCap = 128;
    config.L1MaxCap = 8192;
    config.CacheLifetime = ServiceLifetime.Singleton;
}, config => new InMemory.Module<string>(config));
```

## API Reference

### `Configuration`

Abstract base configuration class for caching modules. Provides common configuration properties for L1/L2 caching and service lifetime.

### `Module<TValue, TConfiguration>`

Abstract base module for registering `IOrderedCache<TValue>`. 

**Type Parameters:**
- `TValue`: The type of values stored in the cache
- `TConfiguration`: Configuration type, must derive from `Configuration`

**Abstract Methods:**
- `BuildL1DataStore(IServiceProvider)`: Build the L1 (fast lookup) data store
- `BuildL2DataStore(IServiceProvider)`: Build the L2 (primary) data store
- `BuildMetadata(IServiceProvider)`: Build the metadata store for cache entry tracking

**Usage:** Extend this class to create custom cache modules with different storage backends.

## Creating Custom Modules

You can create custom cache modules with different storage backends. Modules can be generic (for programmatic loading) or concrete (for configuration-based loading).

### Option 1: Generic Module (Programmatic Loading Only)

Generic modules provide flexibility but can only be loaded programmatically.

#### 1. Define Configuration

```csharp
using Baubit.Caching.DI;

namespace MyApp.Caching
{
    public class RedisConfiguration : Configuration
    {
        public string ConnectionString { get; set; }
        public int DatabaseNumber { get; set; } = 0;
    }
}
```

#### 2. Implement Generic Module

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
    public class RedisModule<TValue> : Module<TValue, RedisConfiguration>
    {
        public RedisModule(RedisConfiguration configuration, List<IModule> nestedModules = null) 
            : base(configuration, nestedModules) { }

        protected override IStore<TValue> BuildL1DataStore(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return new RedisStore<TValue>(
                Configuration.ConnectionString, 
                Configuration.DatabaseNumber,
                Configuration.L1MinCap,
                Configuration.L1MaxCap,
                loggerFactory);
        }

        protected override IStore<TValue> BuildL2DataStore(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return new RedisStore<TValue>(
                Configuration.ConnectionString,
                Configuration.DatabaseNumber,
                loggerFactory);
        }

        protected override IMetadata BuildMetadata(IServiceProvider serviceProvider)
        {
            return new RedisMetadata(
                Configuration.ConnectionString,
                Configuration.DatabaseNumber);
        }
    }
}
```

#### 3. Use Programmatically

```csharp
using MyApp.Caching;
using Baubit.DI;
using Microsoft.Extensions.DependencyInjection;

public class AppComponent : Component
{
    protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
    {
        return builder.WithModule<RedisModule<string>, RedisConfiguration>(config =>
        {
            config.ConnectionString = "localhost:6379";
            config.DatabaseNumber = 0;
            config.IncludeL1Caching = true;
            config.CacheLifetime = ServiceLifetime.Singleton;
        }, config => new RedisModule<string>(config));
    }
}
```

### Option 2: Concrete Module (Configuration-Based Loading)

To enable configuration-based loading, create a concrete (non-generic) module by extending a generic module.

#### 1. Create Concrete Module

```csharp
using Baubit.Caching.DI;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace MyApp.Caching
{
    /// <summary>
    /// Concrete Redis cache module for string values.
    /// Can be loaded from appsettings.json using the "redis-string-cache" key.
    /// </summary>
    [BaubitModule("redis-string-cache")]
    public class RedisStringCacheModule : RedisModule<string>
    {
        // Constructor for configuration-based loading
        public RedisStringCacheModule(IConfiguration configuration) 
            : base(BindConfiguration(configuration)) { }
        
        // Constructor for programmatic loading
        public RedisStringCacheModule(RedisConfiguration configuration, List<IModule> nestedModules = null) 
            : base(configuration, nestedModules) { }
        
        private static RedisConfiguration BindConfiguration(IConfiguration configuration)
        {
            var config = new RedisConfiguration();
            configuration.Bind(config);
            return config;
        }
    }
}
```

#### 2. Create Module Registry

```csharp
using Baubit.DI;

namespace MyApp.Caching
{
    /// <summary>
    /// Module registry for MyApp caching modules.
    /// MUST call Register() before UseConfiguredServiceProviderFactory().
    /// </summary>
    [GeneratedModuleRegistry]
    internal static partial class CachingModuleRegistry
    {
        // Register() method will be generated automatically
    }
}
```

#### 3. Register and Use

> **CRITICAL:** You **MUST** call `CachingModuleRegistry.Register()` before module loading. This registers your modules with Baubit.DI's module registry. Forgetting this step will cause your modules to not be found.

```csharp
using MyApp.Caching;

// REQUIRED: Register modules before loading
CachingModuleRegistry.Register();

await Host.CreateApplicationBuilder()
          .UseConfiguredServiceProviderFactory()
          .Build()
          .RunAsync();
```

**appsettings.json:**
```json
{
  "modules": [
    {
      "key": "redis-string-cache",
      "configuration": {
        "connectionString": "localhost:6379",
        "databaseNumber": 0,
        "includeL1Caching": true,
        "l1MinCap": 128,
        "l1MaxCap": 8192,
        "cacheLifetime": "Singleton"
      }
    }
  ]
}
```

## Dependencies

- [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching) - Core caching abstractions
- [Baubit.DI.Extensions](https://github.com/pnagoorkar/Baubit.DI.Extensions) - Dependency injection modularity framework

## License

[MIT](LICENSE)
