using System.Text.RegularExpressions;
using Aiursoft.CSTools.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.Logging;

namespace Aiursoft.IpmiController.Services;

public class ServerMonitor
{
    private readonly ILogger<ServerMonitor> _logger;

    public ServerMonitor(ILogger<ServerMonitor> logger)
    {
        _logger = logger;
    }
    
    
    public async Task StartMonitoring(Server[] servers, CancellationToken token = default)
    {
        _logger.LogInformation("Starting monitoring...");
        var runner = new CommandService();
        foreach (var server in servers)
        {
            _logger.LogInformation($"Initializing server: {server.HostOrIp}...");
            await runner.RunCommandAsync("ipmitool",
                $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} raw 0x30 0xce 0x00 0x16 0x05 0x00 0x00 0x00 0x05 0x00 0x01 0x00 0x00",
                Directory.GetCurrentDirectory());
            await runner.RunCommandAsync("ipmitool",
                $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} raw 0x30 0x30 0x01 0x00",
                Directory.GetCurrentDirectory());
            //await SetFan(server, 100);
        }

        await Task.Delay(20 * 1000, token);

        while (true)
        {
            var profile = GetProfile();
            _logger.LogInformation("Current Profile is {Profile}", profile.Name);

            foreach (var server in servers)
            {
                _ = Task.Run(async () =>
                {
                    var serverTemperature = await GetTemperature(server);
                    var fanSpeed = GetFanSpeedFromTemperature(serverTemperature);
                    var safeFanSpeed = Math.Max(fanSpeed, 8);
                    Console.WriteLine($"Temp is {serverTemperature}, fan should be set to {safeFanSpeed}. {server.HostOrIp}");
                    await SetFan(server, safeFanSpeed);
                }, token);
                await Task.Delay(800, token);
            }
            await Task.Delay(10 * 1000, token);
            if (token.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<int> GetTemperature(Server server)
    {
        var runner = new CommandService();
        var output = await runner.RunCommandAsync("ipmitool",
            $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} sdr type temperature", Directory.GetCurrentDirectory());
        var regex = new Regex(@"Temp\s+\|\s+\w+\s+\|\s+ok\s+\|\s+\d+\.\d+\s+\|\s+(\d+) degrees C");
        var matches = regex.Matches(output.output);
        if (!matches.Any())
        {
            return 100;
        }

        var max = 0;
        foreach (Match match in matches)
        {
            var tempString = match.Groups[1].Value;
            int temp = int.Parse(tempString);

            if (temp > max)
                max = temp;
        }

        return max + server.Offset;
    }

    private async Task SetFan(Server server, int fan)
    {
        if (server.CurrentFanSpeed != fan)
        {
            var runner = new CommandService();
            await runner.RunCommandAsync("ipmitool",
                $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} raw 0x30 0x30 0x02 0xff 0x" +
                fan.ToString("X2"),
                Directory.GetCurrentDirectory());
            server.CurrentFanSpeed = fan;
        }
    }

    private int GetFanSpeedFromTemperature(int temperature)
    {
        var profile = GetProfile();
        if (temperature < profile.DesiredTemperature)
        {
            return 0;
        }

        if (temperature > profile.MaxTemperature)
        {
            return 100;
        }

        return 100 * (temperature - profile.DesiredTemperature) / (profile.MaxTemperature - profile.DesiredTemperature);
    }

    private Profile GetProfile()
    {
        var quite = new Profile
        {
            Name = "Quiet",
            MaxTemperature = 80,
            DesiredTemperature = 45
        };
        var normal = new Profile
        {
            Name = "Normal",
            MaxTemperature = 77,
            DesiredTemperature = 38
        };
        var turbo = new Profile
        {
            Name = "Turbo",
            MaxTemperature = 73,
            DesiredTemperature = 33
        };
        var full = new Profile
        {
            Name = "Full",
            MaxTemperature = 1,
            DesiredTemperature = 0
        };
        
        var now = DateTime.UtcNow;
        if (now.Hour is >= 0 and < 4)
        {
            return normal;
        }
        if (now.Hour is >= 4 and < 12)
        {
            return turbo;
        }
        if (now.Hour is >= 12 and < 16)
        {
            return normal;
        }
        if (now.Hour is >= 16 and <= 24)
        {
            return quite;
        }
        return full;
    }
}