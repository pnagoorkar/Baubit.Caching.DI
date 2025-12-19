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

### Hybrid Loading

Combine with other modules from appsettings.json:

```csharp
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

To create your own cache module with a custom storage backend:

### 1. Define Configuration

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

### 2. Implement Module

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
            // Build Redis-backed L1 store
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
            // Build Redis-backed L2 store (unbounded)
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return new RedisStore<TValue>(
                Configuration.ConnectionString,
                Configuration.DatabaseNumber,
                loggerFactory);
        }

        protected override IMetadata BuildMetadata(IServiceProvider serviceProvider)
        {
            // Use Redis-backed metadata
            return new RedisMetadata(
                Configuration.ConnectionString,
                Configuration.DatabaseNumber);
        }
    }
}
```

### 3. Use in Application

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

## Dependencies

- [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching) - Core caching abstractions
- [Baubit.DI.Extensions](https://github.com/pnagoorkar/Baubit.DI.Extensions) - Dependency injection modularity framework

## License

[MIT](LICENSE)
