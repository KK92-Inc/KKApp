// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Events;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

public class FreezeDO(Freeze freeze) : BaseEntityDO<Freeze>(freeze)
{
    [Required]
    public string Reason { get; set; } = freeze.Reason;

    [Required]
    public DateTimeOffset StartsAt { get; set; } = freeze.StartsAt;

    [Required]
    public DateTimeOffset EndsAt { get; set; } = freeze.EndsAt;

    [Required]
    public UserLightDO User { get; set; } = freeze.User;

    public static implicit operator FreezeDO?(Freeze? freeze) => freeze is null ? null : new(freeze);
}