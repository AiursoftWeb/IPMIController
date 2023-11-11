using Aiursoft.CSTools.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aiursoft.IpmiController.Services;

public class ServerInitializer
{
    private readonly Server[] _servers;
    private readonly ILogger<ServerInitializer> _logger;

    public ServerInitializer(
        IOptions<List<Server>> servers,
        ILogger<ServerInitializer> logger)
    {
        _servers = servers.Value.ToArray();
        _logger = logger;
    }
    
    public async Task Start()
    {
        _logger.LogInformation("Starting initializing {Count }servers...", _servers.Length);
        var runner = new CommandService();
        foreach (var server in _servers)
        {
            _logger.LogInformation($"Initializing server: {server.HostOrIp}...");
            await runner.RunCommandAsync("ipmitool",
                $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} raw 0x30 0xce 0x00 0x16 0x05 0x00 0x00 0x00 0x05 0x00 0x01 0x00 0x00",
                Directory.GetCurrentDirectory());
            await runner.RunCommandAsync("ipmitool",
                $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} raw 0x30 0x30 0x01 0x00",
                Directory.GetCurrentDirectory());
        }
        
        _logger.LogInformation("All servers initialized!");
    }
}