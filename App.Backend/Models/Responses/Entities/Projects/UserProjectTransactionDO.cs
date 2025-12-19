// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
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
public class UserProjectTransactionDO(UserProjectTransaction transaction)
    : BaseEntityDO<UserProjectTransaction>(transaction)
{
    [Required]
    public Guid UserProjectId { get; set; } = transaction.UserProjectId;

    /// <summary>
    /// The user associated with this transaction, if applicable.
    /// </summary>
    public Guid? UserId { get; set; } = transaction.UserId;

    [Required]
    public UserProjectTransactionVariant Type { get; set; } = transaction.Type;

    /// <summary>
    /// The user who performed the action, if applicable.
    /// </summary>
    public UserLightDO? User { get; set; } = transaction.User;

    public static implicit operator UserProjectTransactionDO?(UserProjectTransaction? transaction) =>
        transaction is null ? null : new(transaction);
}
