// ============================================================================
// W2Inc, Amsterdam 2023-2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

/// <summary>
/// Feedback can be given in different ways such as feedback in regards to an entire file
/// or specific sections of a file
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FeedbackKind
{
    /// <summary>
    /// Feedback targets the entire file as a single visible correction.
    /// <para>
    /// E.g: Project requires users to submit a <c>.docx</c> (Word Document).
    /// Depending on the frontend *we may* or *may* not really support
    /// annotations for these files.
    /// </para>
    /// In comparsion, <c>.txt</c>, files are way easier to support annotations with.
    /// Other file formats may require custom renderes, which may not be feasible.
    ///
    /// <para>
    /// For feedback of this variety we only allow 1 feedback on the review and reviewer
    /// is required to download the file for example and apply corrections in an external
    /// editor.
    /// </para>
    /// </summary>
    [JsonPropertyName(nameof(Correction))]
    Correction,

    /// <summary>
    /// Allows for more fine grained reviewing where reviewer is able to feedback on specific
    /// sections of a file
    /// </summary>
    [JsonPropertyName(nameof(Annotation))]
    Annotation,
}
