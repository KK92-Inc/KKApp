// App.Backend/Core/Services/Implementation/OnsiteNetworkPolicy.cs
using System.Net;
using Microsoft.Extensions.Options;
using App.Backend.Core.Services.Interface;
using App.Backend.Core.Services.Options;

namespace App.Backend.Core.Services.Implementation;

public class OnsiteNetworkService(IOptions<OnsiteNetworkOptions> options) : IOnsiteNetworkService
{
    private readonly List<IPNetwork> _networks = [.. options.Value.AllowedRanges.Select(IPNetwork.Parse)];

    public bool IsOnsite(IPAddress? ip)
    {
        if (ip is null) return false;

        // Kestrel often reports IPv4 clients as IPv4-mapped IPv6 (::ffff:192.168.1.5).
        // Unwrap it or every /24, /16, etc. CIDR check silently fails.
        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        return _networks.Any(n => n.Contains(ip));
    }
}