using Aiursoft.CommandFramework;

namespace Aiursoft.IpmiController;

public class Program
{
    public static async Task Main(string[] args)
    {
        var command = new MonitorHandler().BuildAsCommand();
        await new AiursoftCommandApp(command)
            .RunAsync(args);
    }
}
