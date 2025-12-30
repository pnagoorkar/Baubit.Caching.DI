using Baubit.DI;
using ChatApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hostBuilder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings())
                      .UseConfiguredServiceProviderFactory(componentsFactory: () => [new DevComponent()]);

hostBuilder.Services.AddHostedService<ChatClient>();

await hostBuilder.Build().RunAsync();