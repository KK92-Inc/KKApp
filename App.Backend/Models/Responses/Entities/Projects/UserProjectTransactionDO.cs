// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Projects;

/// <summary>
/// Data object representing an activity transaction within a user project.
/// </summary>
public class UserProjectTransactionDO : BaseEntityDO<UserProjectTransaction>
{
    public UserProjectTransactionDO() { }

    public UserProjectTransactionDO(UserProjectTransaction transaction) : base(transaction)
    {
        UserProjectId = transaction.UserProjectId;
        UserId = transaction.UserId;
        Type = transaction.Type;
        User = transaction.User;
    }

    [Required]
    public Guid UserProjectId { get; set; }

    /// <summary>
    /// The user associated with this transaction, if applicable.
    /// </summary>
    [Required]
    public Guid? UserId { get; set; }

    [Required]
    public UserProjectTransactionVariant Type { get; set; }

    /// <summary>
    /// The user who performed the action, if applicable.
    /// </summary>
    [Required]
    public UserLightDO? User { get; set; }

    public static implicit operator UserProjectTransactionDO?(UserProjectTransaction? transaction) =>
        transaction is null ? null : new(transaction);
}
