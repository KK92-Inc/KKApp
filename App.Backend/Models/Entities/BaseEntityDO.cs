// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using App.Backend.Domain;

namespace App.Backend.Models.Entities;

// ============================================================================

/// <summary>
/// Base Data Object used to convert models into data object that can be
/// transfered to other clients.
/// </summary>
/// <remarks>
/// It is important to note that some object needs to eagerload and others lazy
/// </remarks>
/// <typeparam name="T">Any object that is a Base Entity</typeparam>
/// <param name="entity">The entity to create the DO for</param>
public abstract class BaseEntityDO<T>(T entity) where T : BaseEntity
{
    /// <summary>
    /// The unique identifier for this object.
    /// </summary>
    [JsonPropertyOrder(-3), Required]
    public Guid Id { get; set; } = entity.Id;

    /// <summary>
    /// Was created at.
    /// </summary>
    [JsonPropertyOrder(-2), Required]
    public DateTimeOffset CreatedAt { get; set; } = entity.CreatedAt;

    /// <summary>
    /// Updated at.
    /// </summary>
    [JsonPropertyOrder(-1), Required]
    public DateTimeOffset UpdatedAt { get; set; } = entity.UpdatedAt;
}
