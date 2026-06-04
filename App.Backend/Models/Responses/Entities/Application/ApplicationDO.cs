// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Applications;

/// <summary>
/// Data object representing application registration details.
/// </summary>
public class ApplicationDO(Application application) : BaseEntityDO<Application>(application)
{
    [Required]
    public Guid KeycloakId { get; set; } = application.KeycloakId;

    [Required]
    public string Name { get; set; } = application.Name;

    [Required]
    public string ClientId { get; set; } = application.ClientId;

    [Required]
    public string Description { get; set; } = application.Description;

    [Required]
    public bool Enabled { get; set; } = application.Enabled;

    public ICollection<string> RedirectUris { get; set; } = application.RedirectUris;

    [Required]
    public Guid WorkspaceId { get; set; } = application.WorkspaceId;

    public static implicit operator ApplicationDO?(Application? application) => 
        application is null ? null : new(application);
}