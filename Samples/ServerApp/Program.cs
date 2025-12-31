using Baubit.DI;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerApp;

// Baubit Chat Server - gRPC server for distributed caching
// Hosts the OrderedCache service that synchronizes messages across chat clients.

var webAppBuilder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS for gRPC
// This allows gRPC to work over plain HTTP for development/testing
webAppBuilder.WebHost.ConfigureKestrel(options =>
{
    // Listen on HTTP port with HTTP/2 support
    options.ListenLocalhost(49971, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

webAppBuilder = webAppBuilder.UseConfiguredServiceProviderFactory(componentsFactory: () => [new DevComponent()]);

webAppBuilder.Services.AddGrpc();
var app = webAppBuilder.Build();

app.MapGet("/", () => "Server running!");

// Register the gRPC service for OrderedCache operations
app.MapGrpcService<gRPC.Server.OrderedCacheService>();

await app.RunAsync();
