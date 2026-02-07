// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.Models.Responses.Entities.Notifications;

/// <summary>
/// A simple message notification data object.
/// </summary>
/// <param name="Text">The plain text message content.</param>
/// <param name="Html">The HTML formatted message content.</param>
public record MessageDO(string Text, string Html) : NotificationData;
