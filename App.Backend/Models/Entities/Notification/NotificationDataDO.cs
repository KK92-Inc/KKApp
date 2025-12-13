using System.Text.Json;
using System.Text.Json.Serialization;
using App.Backend.Domain.Enums;

namespace App.Backend.Models.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SpotlightDO), typeDiscriminator: nameof(NotifiableVariant.Spotlight))]
[JsonDerivedType(typeof(DefaultNotificationDataDO), typeDiscriminator: nameof(NotifiableVariant.Default))]
public abstract record NotificationDataDO
{
    public static NotificationDataDO? FromJson(string json, NotifiableVariant variant)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Handle flags if necessary, or just check if the flag is present
        if (variant.HasFlag(NotifiableVariant.Spotlight))
        {
            return JsonSerializer.Deserialize<SpotlightDO>(json, options);
        }

        if (variant.HasFlag(NotifiableVariant.Default))
        {
            return JsonSerializer.Deserialize<DefaultNotificationDataDO>(json, options);
        }

        // Add other variants here

        return null;
    }
}
