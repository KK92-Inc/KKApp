// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using App.Backend.Domain;

namespace App.Backend.Models.Responses;

// ============================================================================

/// <summary>
/// Base Data Object used to convert models into data object that can be
/// transfered to other clients.
/// </summary>
/// <remarks>
/// It is important to note that some object needs to eagerload and others lazy
/// </remarks>
/// <typeparam name="T">Any object that is a Base Entity</typeparam>
/// <param name="entity"></param>
public abstract class BaseEntityDO<T> where T : BaseEntity
{
    protected BaseEntityDO() { }

    protected BaseEntityDO(T entity)
    {
        Id = entity.Id;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    [JsonPropertyOrder(-3), Required]
    public Guid Id { get; set; }

    [JsonPropertyOrder(-2), Required]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyOrder(-1), Required]
    public DateTimeOffset UpdatedAt { get; set; }
}
