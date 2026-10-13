// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

/// <summary>
/// Response object describing a user's participation in a trial.
/// </summary>
public class UserTrialDO : BaseEntityDO<UserTrial>
{
    public UserTrialDO() { }

    public UserTrialDO(UserTrial userTrial) : base(userTrial)
    {
        TrialId = userTrial.TrialId;
        State = userTrial.State;
        Outcome = userTrial.Outcome;
        StartedAt = userTrial.StartedAt;
        EndedAt = userTrial.EndedAt;
        User = new(userTrial.User);
    }

    [Required, Description("The trial this is a participation in.")]
    public Guid TrialId { get; set; }

    [Required, Description("Where the user is within the trial.")]
    public UserTrialState State { get; set; }

    [Required, Description("The verdict of staff. Only meaningful once the trial is over for the user.")]
    public TrialOutcome Outcome { get; set; }

    [Description("When the user became active in the trial. Null if they never did.")]
    public DateTimeOffset? StartedAt { get; set; }

    [Description("When the trial ended for the user. Null while they are still in it.")]
    public DateTimeOffset? EndedAt { get; set; }

    [Required]
    public UserLightDO User { get; set; } = null!;
}
