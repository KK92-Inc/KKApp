// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Applications;

/// <summary>
/// Data object returned strictly on initialization containing the plain text OIDC credential secret.
/// </summary>
public class ApplicationWithSecretDO(Application application, string clientSecret) : ApplicationDO(application)
{
    [Required]
    public string ClientSecret { get; set; } = clientSecret;
}