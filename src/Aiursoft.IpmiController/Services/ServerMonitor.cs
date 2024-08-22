using System.Text.RegularExpressions;
using Aiursoft.CSTools.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aiursoft.IpmiController.Services;

public class ServerMonitor : IHostedService
{
    private readonly Server[] _servers;
    private readonly ProfileConfig _profileConfig;
    private readonly ILogger _logger;
    private readonly IpmiExecutorService _ipmiExecutorService;
    private Timer? _timer;

    public ServerMonitor(
        IOptions<List<Server>> servers,
        IOptions<ProfileConfig> profileConfig,
        ILogger<ServerMonitor> logger,
        IpmiExecutorService ipmiExecutorService)
    {
        _servers = servers.Value.ToArray();
        _profileConfig = profileConfig.Value;
        _logger = logger;
        _ipmiExecutorService = ipmiExecutorService;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Background Service is starting");
        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Background Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        await StartMonitoring();
    }
    
    private async Task StartMonitoring(CancellationToken token = default)
    {
        var profile = GetProfile();
        _logger.LogInformation("Current Profile is {Profile}", profile.Name);

        foreach (var server in _servers)
        {
            _ = Task.Run(async () =>
            {
                var serverTemperature = Math.Max(await GetEgt(server), await GetGpu(server));
                var fanSpeed = GetFanSpeedFromTemperature(serverTemperature, profile);
                var safeFanSpeed = Math.Max(fanSpeed, _profileConfig.MinFan);
                
                _logger.LogInformation("Temperature {ServerTemperature}, fan should be set to {SafeFanSpeed}. {HostOrIp} with offset {Offset}", serverTemperature, safeFanSpeed, server.HostOrIp, server.Offset);
                await SetFan(server, safeFanSpeed);
            }, token);
            await Task.Delay(800, token);
        }
    }

    private async Task<int> GetGpu(Server server)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(server.EsxiIp) || string.IsNullOrWhiteSpace(server.EsxiRootPassword))
            {
                _logger.LogTrace("Server {HostOrIp} doesn't have ESXI IP or ESXI Root Password!", server.HostOrIp);
                // Return 0 because some servers don't have GPU.
                return 0;
            }

            var esxiSsh = new EsxiSshService();
            var nvidiaSmiOutput = await esxiSsh.RunWithSshAndTimeout(
                server.EsxiIp,
                "root",
                server.EsxiRootPassword,
                22,
                "nvidia-smi -q");

            // GPU Current Temp                  : 49 C
            var smi = new Regex(@"GPU Current Temp\s+:\s+(\d+) C");
            var smiMatch = smi.Match(nvidiaSmiOutput);
            if (!smiMatch.Success)
            {
                _logger.LogError("Server {HostOrIp} doesn't match the regex!", server.HostOrIp);
                return 100;
            }

            var smiTempString = smiMatch.Groups[1].Value;

            _logger.LogTrace("Server {HostOrIp} GPU temperature is {SmiTempString}", server.HostOrIp, smiTempString);
            return int.Parse(smiTempString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Server {HostOrIp} failed to get GPU temperature!", server.HostOrIp);
            return 100;
        }
    }

    private async Task<int> GetEgt(Server server)
    {
        try
        {
            var (_, stdout, _)= await _ipmiExecutorService.ExecuteCommand(server, "sdr type temperature"); 
            var regex = new Regex(@"Temp\s+\|\s+\w+\s+\|\s+ok\s+\|\s+\d+\.\d+\s+\|\s+(\d+) degrees C");
            var matches = regex.Matches(stdout);
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
        catch (Exception e)
        {
            _logger.LogError(e, "Server {HostOrIp} failed to get EGT temperature!", server.HostOrIp);
            return 100;
        }
    }

    private async Task SetFan(Server server, int fan)
    {
        if (server.CurrentFanSpeed != fan)
        {
            await _ipmiExecutorService.ExecuteCommand(server, $"raw 0x30 0x30 0x02 0xff 0x{fan:X2}");
            server.CurrentFanSpeed = fan;
        }
    }

    private int GetFanSpeedFromTemperature(int temperature, Profile profile)
    {
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
        var allProfiles = GetAllProfiles();
        if (string.IsNullOrWhiteSpace(_profileConfig.Profile?.ToLower()) ||
            _profileConfig.Profile.ToLower() == "auto")
        {
            var now = DateTime.UtcNow;
            if (now.Hour is >= 0 and < 4)
            {
                return allProfiles.Single(t => t.Name == "Normal");
            }
            if (now.Hour is >= 4 and < 12)
            {
                return allProfiles.Single(t => t.Name == "Turbo");
            }
            if (now.Hour is >= 12 and < 16)
            {
                return allProfiles.Single(t => t.Name == "Normal");
            }
            if (now.Hour is >= 16 and <= 24)
            {
                return allProfiles.Single(t => t.Name == "Quiet");
            }
            return allProfiles.Single(t => t.Name == "Full");
        }
        
        return allProfiles.Single(t => t.Name.ToLower() == _profileConfig.Profile.ToLower());

    }

    private Profile[] GetAllProfiles()
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
        return [quite, normal, turbo, full];
    }
}