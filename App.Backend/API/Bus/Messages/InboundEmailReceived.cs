// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Resend;

namespace App.Backend.API.Bus.Messages;

public record InboundEmailReceived(string recipient, string Subject, string? Html, string? Text);
