namespace Aiursoft.IPMIController.Models;

public class Server
{
    public string HostOrIp { get; init; } = string.Empty;
    public string RootPassword { get; init; } = string.Empty;

    public int CurrentFanSpeed { get; set; } = -1;
    public int Offset { get; set; } = 0;
}