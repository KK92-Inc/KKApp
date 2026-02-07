// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// Data object representing a user's cursus enrollment.
/// </summary>
public class UserCursusDO(UserCursus userCursus) : BaseEntityDO<UserCursus>(userCursus)
{
    [Required]
    public EntityObjectState State { get; set; } = userCursus.State;

    [Required]
    public Guid UserId { get; set; } = userCursus.UserId;

    [Required]
    public Guid CursusId { get; set; } = userCursus.CursusId;

    /// <summary>
    /// The user's personalized track/path through the cursus.
    /// </summary>
    public string? Track { get; set; } = userCursus.Track;

    /// <summary>
    /// The cursus the user is enrolled in.
    /// </summary>
    public CursusDO? Cursus { get; set; } = userCursus.Cursus;

    /// <summary>
    /// The enrolled user.
    /// </summary>
    public UserLightDO? User { get; set; } = userCursus.User;

    public static implicit operator UserCursusDO?(UserCursus? userCursus) =>
        userCursus is null ? null : new(userCursus);
}
