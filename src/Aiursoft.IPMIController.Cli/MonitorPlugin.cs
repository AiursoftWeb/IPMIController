using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.CommandFramework.Framework;

public class MonitorPlugin : IPlugin
{
    public CommandHandler[] Install()
    {
        return new CommandHandler[]
        {
            new MonitorHandler(),
        };
    }
}