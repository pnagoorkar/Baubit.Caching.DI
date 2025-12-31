using Baubit.DI;
using ChatApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Baubit Chat - A distributed chat application demonstrating Baubit.Caching
// with gRPC transport for real-time message synchronization across clients.

var hostBuilder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
                      .UseConfiguredServiceProviderFactory(componentsFactory: () => [new DevComponent()]);

hostBuilder.Services.AddHostedService<ChatClient>();

await hostBuilder.Build().RunAsync();