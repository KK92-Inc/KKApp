// App.Backend/Core/Services/Options/OnsiteNetworkOptions.cs
namespace App.Backend.Core.Services.Options;

public class OnsiteNetworkOptions
{
    public const string SectionName = "Network";

    /// <summary>
    /// CIDR ranges considered "onsite". Works for both LAN subnets (e.g. "10.20.0.0/16")
    /// and a single NAT'd public IP (e.g. "203.0.113.42/32"), same mechanism either way.
    /// </summary>
    public List<string> AllowedRanges { get; set; } = [];
}