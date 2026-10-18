// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.API.Utils;

/// <summary>
/// Reads every <see cref="DateTimeOffset"/> (and nullable) in a request body as UTC and writes
/// every one in a response as UTC, see <see cref="UtcDateTime"/>.
/// </summary>
public sealed class UtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTimeOffset().ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime());
    }
}
