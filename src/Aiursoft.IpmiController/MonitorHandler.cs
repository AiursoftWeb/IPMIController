using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        var host = ServiceBuilder
            .CreateCommandHostBuilder<Startup>(verbose)
            .ConfigureServices((context , services)=>
            {
                var servers = context.Configuration.GetSection("Servers");
                services.Configure<List<Server>>(servers);
            })
            .Build();

        await host.RunAsync();
    }
}