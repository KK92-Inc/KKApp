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
public class ApplicationDO : BaseEntityDO<Application>
{
    public ApplicationDO() { }

    public ApplicationDO(Application application) : base(application)
    {
        Name = application.Name;
        ClientId = application.ClientId;
        Description = application.Description;
        Enabled = application.Enabled;
        Scopes = application.Scopes;
        RedirectUris = application.RedirectUris;
        WorkspaceId = application.WorkspaceId;
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string ClientId { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public bool Enabled { get; set; }

    [Required]
    public ICollection<string> Scopes { get; set; } = [];

    [Required]
    public ICollection<string> RedirectUris { get; set; } = [];

    [Required]
    public Guid WorkspaceId { get; set; }

    public static implicit operator ApplicationDO?(Application? application) => 
        application is null ? null : new(application);
}