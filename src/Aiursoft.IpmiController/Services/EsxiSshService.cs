using Renci.SshNet;

namespace Aiursoft.IpmiController.Services;

public class EsxiSshService
{
    public async Task<string> RunWithSshAndTimeout(string host, string username, string password, int port, string command)
    {
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
        var sshTask = RunWithSsh(host, username, password, port, command);
        await Task.WhenAny(timeoutTask, sshTask);
        if (timeoutTask.IsCompleted)
        {
            throw new TimeoutException("SSH command timeout!");
        }
        return await sshTask;
    }
    
    
    private async Task<string> RunWithSsh(string host, string username, string password, int port, string command)
    {
        var kAuth = new KeyboardInteractiveAuthenticationMethod(username);
        var pAuth = new PasswordAuthenticationMethod(username, password);

        kAuth.AuthenticationPrompt += (_, e) =>
        {
            foreach (var prompt in e.Prompts)
            {
                if (prompt.Request.Contains("Password:", StringComparison.InvariantCultureIgnoreCase))
                {
                    prompt.Response = password;
                }
                else if (prompt.Request.Contains("Are you sure you want to continue connecting", StringComparison.InvariantCultureIgnoreCase))
                {
                    prompt.Response = "yes";
                }
                else
                {
                    throw new Exception("Unknown prompt request: " + prompt.Request);
                }
            }
        };

        var connectionInfo = new ConnectionInfo(
            host, 
            port, 
            username, 
            pAuth, 
            kAuth);
        using var sshClient = new SshClient(connectionInfo);
        sshClient.KeepAliveInterval = TimeSpan.FromSeconds(30);
        await sshClient.ConnectAsync(CancellationToken.None);
        var result = sshClient.RunCommand(command);
        return result.Result;
    }
}
