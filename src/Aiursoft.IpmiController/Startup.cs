using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.IpmiController.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aiursoft.IpmiController;

public class Startup : IStartUp
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ServerInitializer>();
        services.AddSingleton<IHostedService, ServerMonitor>();
        services.AddTransient<IpmiExecutorService>();
    }
}