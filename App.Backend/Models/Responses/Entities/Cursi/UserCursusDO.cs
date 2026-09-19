// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities;

// ============================================================================

namespace App.Backend.Models.Responses.Entities.Cursi;

public class UserCursusDO(Cursus cursus) : BaseEntityDO<Cursus>(cursus)
{

    public static implicit operator UserCursusDO?(Cursus? cursus) => cursus is null ? null : new(cursus);
}
