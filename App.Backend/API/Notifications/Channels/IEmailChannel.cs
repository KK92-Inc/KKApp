// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.API.Bus.Messages;

namespace App.Backend.API.Notifications.Channels;

/// <summary>
/// Notification can be sent via email.
/// </summary>
public interface IEmailChannel
{
    /// <summary>
    /// Represents an email notification channel capable of converting notification data into an <see cref="EmailMessage"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="EmailMessage"/> instance containing the email content to be sent.
    /// </returns>
    public EmailMessage ToMail();
}
