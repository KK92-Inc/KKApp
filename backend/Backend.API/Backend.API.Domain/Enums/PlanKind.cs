// ============================================================================
// W2Inc, Amsterdam 2023-2024, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// The different kinds of cursi that exist.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlanKind
{
    /// <summary>
    /// A free plan is a plan that is free to use.
    /// </summary>
    [JsonPropertyName(nameof(Free))]
    Free,

    /// <summary>
    /// A paid plan is a plan that requires a payment to use.
    /// </summary>
    [JsonPropertyName(nameof(Paid))]
    Paid,
}
