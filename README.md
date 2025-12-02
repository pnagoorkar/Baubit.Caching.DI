# Baubit.Caching.DI

[![CircleCI](https://dl.circleci.com/status-badge/img/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/circleci/TpM4QUH8Djox7cjDaNpup5/2zTgJzKbD2m3nXCf5LKvqS/tree/master)
[![codecov](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI/branch/master/graph/badge.svg)](https://codecov.io/gh/pnagoorkar/Baubit.Caching.DI)<br/>
[![NuGet](https://img.shields.io/nuget/v/Baubit.Caching.DI.svg)](https://www.nuget.org/packages/Baubit.Caching.DI/)
![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)<br/>
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Known Vulnerabilities](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI/badge.svg)](https://snyk.io/test/github/pnagoorkar/Baubit.Caching.DI)

Dependency injection modules for [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching). Registers `IOrderedCache<TValue>` in your DI container with configurable L1/L2 caching and service lifetimes.

## Installation

```bash
dotnet add package Baubit.Caching.DI
```

## Quick Start

```csharp
using Baubit.DI;
using Baubit.DI.Extensions;
using Baubit.Caching.DI.InMemory;
using Microsoft.Extensions.DependencyInjection;

// Register an in-memory ordered cache
var componentResult = ComponentBuilder.CreateNew()
    .WithModule<Module<string>, Configuration>(config =>
    {
        config.CacheLifetime = ServiceLifetime.Singleton;
    });

// Load into service collection
var services = new ServiceCollection();
services.AddLogging();
foreach (var module in componentResult.Value.Build().Value)
{
    module.Load(services);
}

// Resolve and use
var provider = services.BuildServiceProvider();
var cache = provider.GetRequiredService<IOrderedCache<string>>();
```

## Features

- **L1/L2 Caching**: Optional bounded L1 (fast lookup) layer with unbounded L2 storage
- **Configurable Lifetimes**: Singleton, Transient, or Scoped registration
- **IConfiguration Support**: Load settings from appsettings.json or other configuration sources

## Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncludeL1Caching` | `bool` | `false` | Enable bounded L1 caching layer |
| `L1MinCap` | `int` | `128` | Minimum capacity for L1 store |
| `L1MaxCap` | `int` | `8192` | Maximum capacity for L1 store |
| `CacheConfiguration` | `Configuration` | `null` | Underlying cache configuration |
| `CacheLifetime` | `ServiceLifetime` | `Singleton` | DI service lifetime |

## API Reference

### `AConfiguration`

Abstract base configuration class for caching modules.

### `AModule<TValue, TConfiguration>`

Abstract base module for registering `IOrderedCache<TValue>`. Implement `BuildL1DataStore`, `BuildL2DataStore`, and `BuildMetadata` to customize cache construction.

### `InMemory.Module<TValue>`

Concrete module using in-memory stores. L1 uses bounded `Store<TValue>` with capacity limits. L2 uses unbounded `Store<TValue>`.

## Dependencies

- [Baubit.Caching](https://github.com/pnagoorkar/Baubit.Caching)
- [Baubit.DI.Extensions](https://github.com/pnagoorkar/Baubit.DI.Extensions)

## License

[MIT](LICENSE)