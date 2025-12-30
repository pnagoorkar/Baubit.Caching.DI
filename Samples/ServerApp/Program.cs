using Baubit.DI;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerApp;

var webAppBuilder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS for gRPC
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

app.MapGrpcService<gRPC.Server.OrderedCacheService>();

await app.RunAsync();
