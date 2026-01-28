// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class NotificationService(DatabaseContext context) : BaseService<Notification>(context), INotificationService
{
    public Task MarkAsAsync(Guid userId, IEnumerable<Guid>? guids = null, bool read = true, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

}
