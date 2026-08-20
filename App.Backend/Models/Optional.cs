// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

// ============================================================================

namespace App.Backend.Models;

/// <summary>
/// Non-generic marker so the JSON/validation/OpenAPI infrastructure can
/// recognize an <see cref="Optional{T}"/> property without needing to know
/// its generic argument up front.
/// </summary>
public interface IOptional
{
    bool HasValue { get; }
    object? BoxedValue { get; }
    Type ValueType { get; }
}

/// <summary>
/// Represents a value that is either "not present" or "present with a value",
/// this is a different axis than <c>T?</c>, which only distinguishes "no
/// value" from "a value" and has no way to represent "the client didn't send
/// this key at all" separately from "the client sent this key as null".
/// </summary>
[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly struct Optional<T>(T value) : IOptional, IEquatable<Optional<T>>
{
    private readonly T _value = value;

    public bool HasValue { get; } = true;

    /// <summary>
    /// The underlying value. Returns default(T) if HasValue is false to prevent 
    /// framework reflection (like ASP.NET Core ValidationVisitor) from throwing.
    /// </summary>
    public T Value => _value;

    /// <summary>The "not present" value. Equivalent to <c>default</c>.</summary>
    public static Optional<T> None => default;

    /// <summary>Lets you write <c>Optional&lt;string&gt; x = "foo";</c> directly, e.g. in tests.</summary>
    public static implicit operator Optional<T>(T value) => new(value);

    public T GetValueOrDefault() => _value;

    public T GetValueOrDefault(T fallback) => HasValue ? _value : fallback;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }

    object? IOptional.BoxedValue => HasValue ? _value : null;
    Type IOptional.ValueType => typeof(T);

    public bool Equals(Optional<T> other) =>
        HasValue == other.HasValue && (!HasValue || EqualityComparer<T>.Default.Equals(_value, other._value));

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);
    public override int GetHashCode() => HasValue ? EqualityComparer<T>.Default.GetHashCode(_value!) : 0;
    public override string ToString() => HasValue ? $"{_value}" : "<not set>";

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}