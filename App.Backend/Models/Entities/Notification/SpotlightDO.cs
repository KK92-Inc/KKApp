using App.Backend.Models.Entities;

namespace App.Backend.Models.Entities;

public record SpotlightDO(
    string Title,
    string Description,
    string ActionText,
    string Href,
    string BackgroundUrl
) : NotificationDataDO;
