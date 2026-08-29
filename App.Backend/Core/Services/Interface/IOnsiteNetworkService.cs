// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

namespace App.Backend.Core.Services.Interface;

public interface IOnsiteNetworkService
{
    bool IsOnsite(System.Net.IPAddress? remoteIp);
}