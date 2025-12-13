using App.Backend.Models.Entities;

namespace App.Backend.Models.Entities;

public record DefaultNotificationDataDO(
    string Message
) : NotificationDataDO;
