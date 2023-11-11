namespace Aiursoft.IpmiController.Models;

public class Profile
{
    public required string Name { get; init; }
    public int MaxTemperature { get; init; }
    public int DesiredTemperature { get; init; }
}