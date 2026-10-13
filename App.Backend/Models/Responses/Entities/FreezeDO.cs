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

public class FreezeDO : BaseEntityDO<Freeze>
{
    public FreezeDO() { }

    public FreezeDO(Freeze freeze) : base(freeze)
    {
        Reason = freeze.Reason;
        StartsAt = freeze.StartsAt;
        EndsAt = freeze.EndsAt;
        User = freeze.User;
    }

    [Required]
    public string Reason { get; set; } = null!;

    [Required]
    public DateTimeOffset StartsAt { get; set; }

    [Required]
    public DateTimeOffset EndsAt { get; set; }

    [Required]
    public UserLightDO User { get; set; } = null!;

    public static implicit operator FreezeDO?(Freeze? freeze) => freeze is null ? null : new(freeze);
}