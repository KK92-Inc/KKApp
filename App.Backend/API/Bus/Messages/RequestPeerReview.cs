// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Backend.API.Bus.Messages;

public record RequestPeerReview(Guid ReviewId, Guid UserProjectId);
