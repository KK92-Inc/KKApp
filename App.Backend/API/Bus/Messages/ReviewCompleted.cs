// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;

namespace App.Backend.API.Bus.Messages;

public record ReviewCompleted(Guid ReviewId);
