// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{

    [JsonPropertyName(nameof(Applicant))]
    Applicant = 1,

    [JsonPropertyName(nameof(Student))]
    Student = 2,

    [JsonPropertyName(nameof(Staff))]
    Staff = 3,
}

