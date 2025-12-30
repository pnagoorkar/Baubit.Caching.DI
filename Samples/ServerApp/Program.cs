using Baubit.DI;
using ServerApp;

var webAppBuilder = WebApplication.CreateBuilder(args)
                                  .UseConfiguredServiceProviderFactory(componentsFactory: () => [new DevComponent()]);

webAppBuilder.Services.AddGrpc();
var app = webAppBuilder.Build();

app.MapGet("/", () => "Server running!");

app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapGrpcService<gRPC.Server.OrderedCacheService>(); });

await app.RunAsync();
