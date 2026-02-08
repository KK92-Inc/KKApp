// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Enums;
using App.Backend.Domain.Relations;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursus;

/// <summary>
/// Response DTO representing the user's personalized view of a cursus track.
/// Each goal node includes the user's computed progress state.
/// </summary>
public class UserCursusTrackDO
{
    /// <summary>
    /// The cursus ID this track belongs to.
    /// </summary>
    [Required]
    public Guid CursusId { get; set; }

    /// <summary>
    /// The variant (Fixed or Dynamic) of the cursus.
    /// </summary>
    [Required]
    public CursusVariant Variant { get; set; }

    /// <summary>
    /// The user ID whose state is reflected.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// The hierarchical tree of goal nodes with user state.
    /// </summary>
    public IList<UserCursusTrackNodeDO> Nodes { get; set; } = [];

    /// <summary>
    /// Build from cursus track relations and user goal states.
    /// </summary>
    public static UserCursusTrackDO FromRelations(
        Domain.Entities.Cursus cursus,
        Guid userId,
        IEnumerable<CursusGoal> relations,
        IReadOnlyDictionary<Guid, EntityObjectState> userGoalStates)
    {
        return new UserCursusTrackDO
        {
            CursusId = cursus.Id,
            Variant = cursus.Variant,
            UserId = userId,
            Nodes = UserCursusTrackNodeDO.BuildTree(relations, userGoalStates)
        };
    }
}
