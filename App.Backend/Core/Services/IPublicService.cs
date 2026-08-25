// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Domain;

// ============================================================================

namespace App.Backend.Core.Services;

/// <summary>
/// Entity can be hidden in public.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPublicService<T> where T : BaseEntity
{

}
