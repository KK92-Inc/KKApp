// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

// ============================================================================

namespace App.Backend.Models;

/// <summary>
/// Resolves the closed <see cref="OptionalJsonConverter{T}"/> for any
/// <see cref="Optional{T}"/>, whatever T happens to be.
/// </summary>
public sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(innerType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// <para>
/// The entire "did the client send this key" trick lives in the fact that
/// System.Text.Json only invokes <see cref="Read"/> for a property when that
/// property's key is actually present in the incoming JSON object. If the key
/// is missing from the payload, the property is left at its CLR default —
/// which for a struct like <see cref="Optional{T}"/> is <see cref="Optional{T}.None"/>.
/// We don't have to do anything special to detect "absent"; STJ does it for us.
/// </para>
/// <para>
/// See the XML doc on <see cref="Optional{T}"/> for why an explicit JSON
/// <c>null</c> is rejected rather than silently accepted.
/// </para>
/// </summary>
public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException(
                $"Property of type Optional<{typeof(T).Name}> does not accept an explicit JSON null. " +
                "Omit the property from the request body instead. (If you need a genuine " +
                "\"clear this field\" state, model the property as Optional<T?> for a value type T " +
                "— e.g. Optional<int?> — which works out of the box.)");
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options)!;
        return new Optional<T>(value);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        // Properties with HasValue == false are skipped entirely before this
        // is ever called for object properties — see AddOptionalSupport()
        // below. This branch only matters if you construct one directly
        // (e.g. `Optional<string> x = null;`) and serialize it standalone.
        if (value.HasValue)
            JsonSerializer.Serialize(writer, value.Value, options);
        else
            writer.WriteNullValue();
    }
}

/// <summary>
/// Wires <see cref="Optional{T}"/> support into a <see cref="JsonSerializerOptions"/>:
/// the converter factory, plus a contract modifier that omits any
/// <c>HasValue == false</c> property from serialized output entirely (instead
/// of writing <c>"prop": null</c>), for the case an <see cref="Optional{T}"/>
/// ends up on a response type as well as a request type.
/// </summary>
public static class OptionalJsonExtensions
{
    public static JsonSerializerOptions AddOptionalSupport(this JsonSerializerOptions options)
    {
        options.Converters.Add(new OptionalJsonConverterFactory());
        options.TypeInfoResolver = (options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
            .WithAddedModifier(HideAbsentOptionalProperties);
        return options;
    }

    private static void HideAbsentOptionalProperties(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            if (typeof(IOptional).IsAssignableFrom(property.PropertyType))
                property.ShouldSerialize = (_, value) => value is IOptional { HasValue: true };
        }
    }
}