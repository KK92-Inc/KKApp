// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

// ============================================================================

namespace App.Backend.Tests.Integration;

/// <summary>
/// Modified JSON Options to account for primary constructor requiring a object
/// </summary>
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { UseUninitializedObjectCreation }
        }
    };

    private static void UseUninitializedObjectCreation(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind is not JsonTypeInfoKind.Object)
            return;
        if (typeInfo.Type.Namespace?.StartsWith("App.Backend.Models.Responses") is not true)
            return;

        typeInfo.CreateObject = () => RuntimeHelpers.GetUninitializedObject(typeInfo.Type);
    }
}
