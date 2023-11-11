using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.IpmiController.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.IpmiController;

public class Startup : IStartUp
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ServerMonitor>();
    }
}