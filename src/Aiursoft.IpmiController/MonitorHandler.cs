using System.CommandLine;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aiursoft.IpmiController;

public class MonitorHandler : CommandHandler
{
    public override string Name => "monitor";

    public override string Description => "Monitor the temperature of your servers.";
    
    private readonly Option<string> _profile = new(
        aliases: new[] { "--profile", "-p" },
        getDefaultValue: () => "auto",
        description: "The target profile. Can be: 'auto','turbo','normal','quiet','full'.")
    {
        IsRequired = false
    };

    public override Option[] GetCommandOptions()
    {
        return new Option[]
        {
            _profile
        };
    }

    public override void OnCommandBuilt(Command command)
    {
        command.SetHandler(
            Execute, CommonOptionsProvider.VerboseOption, _profile);
    }

    private async Task Execute(bool verbose, string profile)
    {
        var host = ServiceBuilder
            .CreateCommandHostBuilder<Startup>(verbose)
            .ConfigureServices((context , services)=>
            {
                var servers = context.Configuration.GetSection("Servers");
                services.Configure<List<Server>>(servers);
                services.Configure<ProfileConfig>(config =>
                {
                    config.Profile = profile;
                });
            })
            .Build();

        await host.RunAsync();
    }
}