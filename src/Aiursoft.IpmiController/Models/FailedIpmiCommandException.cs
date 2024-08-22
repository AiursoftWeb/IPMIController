namespace Aiursoft.IpmiController.Models;

public class FailedIpmiCommandException(Server server, string command, int code, string stdout, string stderr)
    : Exception
{
    public Server Server { get; } = server;
    public string Command { get; } = command;
    public int Code { get; } = code;
    public string Stdout { get; } = stdout;
    public string Stderr { get; } = stderr;

    public override string Message =>
        $"Failed to execute IPMI command on server {Server.HostOrIp} with code {Code}.\nCommand: {Command}\nStdout: {Stdout}\nStderr: {Stderr}";
}