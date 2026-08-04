// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain.Entities.Reviews;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================

namespace App.Backend.Domain.Entities;

/// <summary>
/// System table to indicate if app has been bootstrapped.
/// </summary>
[Table("system", Schema = "internal")]
public class System : BaseEntity { }
