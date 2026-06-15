
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace App.Backend.Tests.Integration.Entities;

public static class DOJsonOptions
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
