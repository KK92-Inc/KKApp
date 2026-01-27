// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.API.Views.Models;

namespace App.Backend.API.Notifications.Channels;

// ============================================================================

/// <summary>
/// Interface that mandates that the notification is to be sent via Email.
/// </summary>
/// <typeparam name="T">The View Model</typeparam>
public interface IEmailChannel<T> where T : BaseViewModel
{
    /// <summary>
    /// The subject of the Email.
    /// </summary>
    public abstract string Subject { get; }

    /// <summary>
    /// The view to render the email with.
    /// </summary>
    public abstract string View { get; }

    /// <summary>
    /// The model to apply.
    /// </summary>
    public abstract T Model { get; }
}
