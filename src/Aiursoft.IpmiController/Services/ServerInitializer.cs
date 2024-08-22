using Aiursoft.CSTools.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aiursoft.IpmiController.Services;

public class ServerInitializer
{
    private readonly Server[] _servers;
    private readonly ILogger<ServerInitializer> _logger;
    private readonly IpmiExecutorService _ipmiExecutorService;

    public ServerInitializer(
        IOptions<List<Server>> servers,
        ILogger<ServerInitializer> logger,
        IpmiExecutorService ipmiExecutorService)
    {
        _servers = servers.Value.ToArray();
        _logger = logger;
        _ipmiExecutorService = ipmiExecutorService;
    }
    
    public async Task Start()
    {
        _logger.LogInformation("Starting initializing {Count }servers...", _servers.Length);
        var runner = new CommandService();
        foreach (var server in _servers)
        {
            try
            {
                _logger.LogInformation($"Initializing server: {server.HostOrIp}...");
                // Disable maximum fan speed when GPU is present.
                await _ipmiExecutorService.ExecuteCommand(server,
                    "raw 0x30 0xce 0x00 0x16 0x05 0x00 0x00 0x00 0x05 0x00 0x01 0x00 0x00");
                // Disable system automatic fan control
                await _ipmiExecutorService.ExecuteCommand(server, "raw 0x30 0x30 0x01 0x00");
            }
            catch (FailedIpmiCommandException ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError("Failed to initialize server: {HostOrIp}, ignoring...", server.HostOrIp);
            }
            
        }
        
        _logger.LogInformation("All servers initialized!");
    }
    
    public async Task Finalize()
    {
        _logger.LogInformation("Starting finalizing {Count }servers...", _servers.Length);
        var runner = new CommandService();
        foreach (var server in _servers)
        {
            try
            {
                _logger.LogInformation($"Finalizing server: {server.HostOrIp}...");
                // The server fan will jump to 100% when system shutdown if I do so...
                // await _ipmiExecutorService.ExecuteCommand(server,
                //     "raw 0x30 0xce 0x00 0x16 0x05 0x00 0x00 0x00 0x05 0x00 0x00 0x00 0x00");
                await _ipmiExecutorService.ExecuteCommand(server, "raw 0x30 0x30 0x01 0x01");
            }
            catch (FailedIpmiCommandException ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError("Failed to finalize server: {HostOrIp}, ignoring...", server.HostOrIp);
            }
        }
        _logger.LogInformation("All servers finalized!");
    }
}