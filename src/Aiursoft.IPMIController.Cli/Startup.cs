using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.IPMIController.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

public class Startup : IStartUp
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ServerMonitor>();
    }
}