// ============================================================================
// W2Inc, Amsterdam 2023-2025, All Rights Reserved.
// See README in the root project for more information.
// ============================================================================

using System.Linq.Expressions;

// ============================================================================

namespace Backend.API.Core.Query;

public static class Filter
{
    // 1. Handle Strings (checks Null OR Empty)
    public static Expression<Func<T, bool>>? With<T>(string? value, Expression<Func<T, bool>> filter)
    {
        return !string.IsNullOrEmpty(value) ? filter : null;
    }

    // 2. Handle Nullable Structs (int?, DateTime?, Guid?, etc.)
    public static Expression<Func<T, bool>>? With<T, TValue>(TValue? value, Expression<Func<T, bool>> filter)
        where TValue : struct
    {
        return value.HasValue ? filter : null;
    }

    // 3. Handle Reference Types (Classes, Arrays, Lists)
    public static Expression<Func<T, bool>>? With<T, TValue>(TValue? value, Expression<Func<T, bool>> filter)
        where TValue : class
    {
        return value != null ? filter : null;
    }
}
