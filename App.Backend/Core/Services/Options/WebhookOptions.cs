// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Core.Services.Options;

/// <summary>
/// Configuration options for webhook secrets
/// </summary>
public class WebhookOptions
{
    public const string SectionName = "Webhooks";

    public required string ResendSecret { get; set; }
}
