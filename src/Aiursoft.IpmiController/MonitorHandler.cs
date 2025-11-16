using System.CommandLine;
using System.CommandLine.Invocation;
using Aiursoft.CommandFramework.Framework;
using Aiursoft.CommandFramework.Models;
using Aiursoft.CommandFramework.Services;
using Aiursoft.IpmiController.Models;
using Aiursoft.IpmiController.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aiursoft.IpmiController;

public class MonitorHandler : ExecutableCommandHandlerBuilder
{
    protected override string Name => "monitor";

    protected override string Description => "Monitor the temperature of your servers.";
    
    private readonly Option<string> _profile = new(
        aliases: ["--profile", "-p"],
        getDefaultValue: () => "auto",
        description: "The target profile. Can be: 'auto','turbo','normal','quiet','full'.")
    {
        IsRequired = false
    };
    
    private readonly Option<int> _minFan = new(
        aliases: ["--minfan", "-m"],
        getDefaultValue: () => 6,
        description: "The minimum fan speed. Can be: 0-100.")
    {
        IsRequired = false
    };

    protected override async Task Execute(ParseResult context)
    {
        var verbose = context.GetValue(CommonOptionsProvider.VerboseOption);
        var profile = context.GetValue(_profile);
        var minFan = context.GetValue(_minFan);
        
        var host = ServiceBuilder
            .CreateCommandHostBuilder<Startup>(verbose)
            .ConfigureServices((hostBuilderContext , services)=>
            {
                var servers = hostBuilderContext.Configuration.GetSection("Servers");
                services.Configure<List<Server>>(servers);
                services.Configure<ProfileConfig>(config =>
                {
                    config.Profile = profile;
                    config.MinFan = minFan;
                });
            })
            .Build();

        await host.StartAsync();
        
        var serverInitializer = host.Services.GetRequiredService<ServerInitializer>();
        await serverInitializer.Start();
        
        await host.WaitForShutdownAsync();
        await serverInitializer.FinalizeServers();
    }

    protected override Option[] GetCommandOptions()
    {
        return
        [
            CommonOptionsProvider.VerboseOption,
            _profile,
            _minFan
        ];
    }
}