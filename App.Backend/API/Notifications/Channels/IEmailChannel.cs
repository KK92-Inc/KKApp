// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Net.Mail;
using App.Backend.API.Views.Models;

namespace App.Backend.API.Notifications.Channels;

// ============================================================================

/// <summary>
/// Interface that mandates that the notification is to be sent via Email.
/// </summary>
/// <typeparam name="T">The View Model</typeparam>
public interface IEmailChannel
{
    public MailMessage ToMail();
}
