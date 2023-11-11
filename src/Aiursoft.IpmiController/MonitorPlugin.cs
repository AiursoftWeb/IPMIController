using Aiursoft.CommandFramework.Abstracts;
using Aiursoft.CommandFramework.Framework;

namespace Aiursoft.IpmiController;

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