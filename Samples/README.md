# Baubit Chat - Distributed Caching Sample

Real-time distributed chat application demonstrating Baubit.Caching with gRPC transport. Messages are synchronized across multiple chat clients through a centralized cache server.

## Architecture

```mermaid
graph TB
    %% --- Server on top ---
    Server["ServerApp<br/>(Server)"]

    %% --- Clients row ---
    subgraph Clients[" "]
        direction LR

        Client1["ChatApp<br/>(Client 1)"]
        Client2["ChatApp<br/>(Client 2)"]

        Dots["⋯"]

        ClientN["ChatApp<br/>(Client N)"]
    end

    %% --- Connections ---
    Server <-->|gRPC / HTTP2| Client1
    Server <-->|gRPC / HTTP2| Client2
    Server <-->|gRPC / HTTP2| ClientN

    %% --- Centered note below clients ---
    Note["All clients<br/>synchronize via<br/>IOrderedCache&lt;long, ChatMessage&gt;"]
    Clients --- Note

    %% --- Styling ---
    style Client1 fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style Client2 fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style ClientN fill:#e3f2fd,stroke:#1976d2,stroke-width:2px

    style Server fill:#fff3e0,stroke:#f57c00,stroke-width:2px

    style Note fill:#f5f5f5,stroke:#9e9e9e,stroke-dasharray:5 5

    %% --- Make dots unobtrusive ---
    style Dots fill:transparent,stroke:transparent,color:#666
```

**Components:**

- **ServerApp**: gRPC server hosting an in-memory `IOrderedCache<long, byte[]>` exposed via gRPC endpoints
- **ChatApp**: Console chat client using gRPC client to connect to the distributed cache
- **gRPC**: Shared library containing proto definitions, client/server modules, and MessagePack serialization

## Quick Start

### Terminal 1 - Start Server

```bash
cd Samples/ServerApp
dotnet run
```

Server starts on `http://localhost:49971` with HTTP/2 support.

### Terminal 2 - First Chat Client

```bash
cd Samples/ChatApp
dotnet run
```

Enter a username (e.g., "Alice") and start chatting.

### Terminal 3 - Second Chat Client

```bash
cd Samples/ChatApp
dotnet run
```

Enter a different username (e.g., "Bob") and start chatting.

Messages sent by one client appear in real-time on all other connected clients.

## Features

- **Real-time messaging**: Messages appear instantly across all connected clients via gRPC streaming
- **Distributed cache**: Leverages `IOrderedCache<TId, TValue>` for message synchronization
- **Sequential IDs**: Server uses `InMemory.Long.Module<byte[]>` for auto-incrementing message IDs
- **MessagePack serialization**: Efficient binary serialization for gRPC transport
- **HTTP/2 protocol**: gRPC over plain HTTP for development/testing

## How It Works

1. **Server** maintains an in-memory ordered cache storing serialized chat messages
2. **Clients** connect via gRPC and use `IOrderedCache<long, ChatMessage>` interface
3. **Adding messages**: `cache.Add()` sends messages to server via gRPC `Add` RPC
4. **Receiving messages**: `cache.GetFutureAsyncEnumerator()` streams new messages via gRPC `EnumerateFuture` RPC
5. **Synchronization**: All clients receive messages in order through the distributed cache

## Key Code Components

### ChatClient.cs
- `StartListeningAsync()`: Subscribes to future messages via async enumeration
- `RunPostingLoop()`: Reads user input and adds messages to cache
- `DisplayBanner()`: Shows ASCII art banner on startup

### ServerApp/Program.cs
- Configures Kestrel for HTTP/2 over plain HTTP
- Registers gRPC service and in-memory cache module

### gRPC Library
- **Client Module**: Wraps gRPC calls in `IOrderedCache<TId, TValue>` interface
- **Server Service**: Exposes cache operations via gRPC endpoints
- **Serialization**: MessagePack for efficient message encoding

## Configuration

**Server Port**: `49971` (configured in `ServerApp/Program.cs` and `gRPC/Client/DI/Configuration.cs`)

To change the port:
1. Update `ServerApp/Program.cs`: `options.ListenLocalhost(YOUR_PORT, ...)`
2. Update `gRPC/Client/DI/Configuration.cs`: `GrpcChannelAddress = "http://localhost:YOUR_PORT"`

## Technology Stack

- **.NET 9.0**: Console and web applications
- **ASP.NET Core**: gRPC server hosting
- **Grpc.AspNetCore**: gRPC server implementation
- **Grpc.Net.Client**: gRPC client implementation
- **MessagePack**: Binary serialization
- **Baubit.Caching**: Distributed caching abstractions
- **Baubit.DI.Extensions**: Dependency injection modularity

## Troubleshooting

**gRPC connection error**: Ensure server is running on port 49971 with HTTP/2 support enabled.

**Messages not appearing**: Verify both clients are connected to the same server instance.

**Port already in use**: Change the port number in both server and client configurations.
