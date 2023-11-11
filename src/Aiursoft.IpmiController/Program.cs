// Program.cs
using Aiursoft.CommandFramework;
using Aiursoft.CommandFramework.Extensions;
using Aiursoft.IpmiController.Services;

namespace Aiursoft.IpmiController;

public class Program
{
    public static async Task Main(string[] args)
    {
        await new AiursoftCommand()
            .Configure(command =>
            {
                command
                    .AddGlobalOptions()
                    .AddPlugins(new MonitorPlugin());
            })
            .RunAsync(args);
    }
}
