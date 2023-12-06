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
    public override string Name => "monitor";

    public override string Description => "Monitor the temperature of your servers.";
    
    private readonly Option<string> _profile = new(
        aliases: new[] { "--profile", "-p" },
        getDefaultValue: () => "auto",
        description: "The target profile. Can be: 'auto','turbo','normal','quiet','full'.")
    {
        IsRequired = false
    };
    
    private readonly Option<int> _minFan = new(
        aliases: new[] { "--minfan", "-m" },
        getDefaultValue: () => 6,
        description: "The minimum fan speed. Can be: 0-100.")
    {
        IsRequired = false
    };

    protected override async Task Execute(InvocationContext context)
    {
        var verbose = context.ParseResult.GetValueForOption(CommonOptionsProvider.VerboseOption);
        var profile = context.ParseResult.GetValueForOption(_profile);
        var minFan = context.ParseResult.GetValueForOption(_minFan);
        
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
    }

    public override Option[] GetCommandOptions()
    {
        return new Option[]
        {
            CommonOptionsProvider.VerboseOption,
            _profile,
            _minFan
        };
    }
}