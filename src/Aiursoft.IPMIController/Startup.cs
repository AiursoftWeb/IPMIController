using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.IPMIController.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.IPMIController;

public class Startup : IStartUp
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ServerMonitor>();
    }
}