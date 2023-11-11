using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.IpmiController.Models;
using Aiursoft.IpmiController.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.IpmiController;

public class MonitorHandler : CommandHandler
{
    public override string Name => "monitor";

    public override string Description => "Monitor the temperature of your servers.";

    public override void OnCommandBuilt(Command command)
    {
        command.SetHandler(
            Execute, CommonOptionsProvider.VerboseOption);
    }

    private async Task Execute(bool verbose)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        
        var servers = configuration.GetSection("Servers").Get<Server[]>();
        
        var host = ServiceBuilder
            .BuildHost<Startup>(verbose)
            .Build();

        await host.StartAsync();
        
        var serverMonitor = host.Services.GetRequiredService<ServerMonitor>();
        await serverMonitor.StartMonitoring(servers!, CancellationToken.None);
    }
}