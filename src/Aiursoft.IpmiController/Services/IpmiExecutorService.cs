using Aiursoft.CSTools.Services;
using Aiursoft.IpmiController.Models;
using Microsoft.Extensions.Logging;

namespace Aiursoft.IpmiController.Services;

// Reference: https://github.com/ipmitool/ipmitool/issues/30

public class IpmiExecutorService
{
    private readonly ILogger<IpmiExecutorService> _logger;
    private readonly CommandService _runner;

    public IpmiExecutorService(ILogger<IpmiExecutorService> logger)
    {
        _logger = logger;
        _runner = new CommandService();
    }

    /// <summary>
    /// Executes an IPMI command on the specified server.
    /// </summary>
    /// <param name="server">The server on which to execute the command.</param>
    /// <param name="options">The options to pass to the IPMI tool command.</param>
    /// <param name="ensureSuccess">If true, retries the command until it succeeds (code = 0) or the maximum number of tries is reached.</param>
    /// <param name="maxTries">The maximum number of attempts to execute the command. -1 = infinity</param>
    /// <exception cref="InvalidOperationException">Thrown if the command fails after the maximum number of attempts.</exception>
    public async Task<(int code, string output, string error)> ExecuteCommand(Server server, string options, bool ensureSuccess = true, int maxTries = 3)
    {
        FailedIpmiCommandException? except = null;
        while (maxTries-- != 0)
        {
            var result = await _runner.RunCommandAsync("ipmitool",
                $"-I lanplus -H {server.HostOrIp} -U root -P {server.RootPassword} {options}",
                Directory.GetCurrentDirectory());
            if (result.code == 0 || !ensureSuccess) return result;
            _logger.LogWarning("Ipmi command {Options} failed! Chance left: {Chance}", options, maxTries);
            except = new FailedIpmiCommandException(server, options, result.code, result.output, result.error);
        }
        _logger.LogError("Ipmi command {Options} failed, throwing.", options);
        throw except!;
    }
}
