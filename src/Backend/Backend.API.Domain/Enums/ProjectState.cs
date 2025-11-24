using System;
using System.Text.Json.Serialization;

namespace Backend.API.Domain.Enums;

[Flags] // TODO: Use this over bools
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectState
{
    None = 0,
    Public = 1 << 0,
    Enabled = 1 << 1,
    Deprecated = 1 << 2
}
