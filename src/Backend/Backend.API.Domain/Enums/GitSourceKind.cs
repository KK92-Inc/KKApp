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
public enum GitProviderKind
{
    /// <summary>
    /// External source control other than Github. E.g: (Gitea, Gitlab, Custom Home server, ...)
    /// TODO: Not implemented yet
    /// </summary>
    [JsonPropertyName(nameof(Other))]
    Other,

    /// <summary>
    /// Managed referring to the self running instance of the application.
    /// </summary>
    [JsonPropertyName(nameof(Managed))]
    Managed,

    /// <summary>
    /// Uses github integration to manage the git source control, this is essentially the same as
    /// third party in essence except that because github is the more popular variant we *may* want
    /// to offer some more features that they provide.
    ///
    /// TODO: Bundle it simply with other or offer a more tight integration ?
    /// </summary>
    // [JsonPropertyName(nameof(Github))]
    // Github,
}
