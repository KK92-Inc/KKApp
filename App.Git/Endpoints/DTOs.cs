// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

namespace App.Git.Endpoints;

/// <summary>
/// Request/Response DTOs for the API.
/// </summary>
/// 
// Repository DTOs
public record CreateRepositoryRequest(
    string Name,
    string? Description = null,
    string Visibility = "private"
);

public record UpdateRepositoryRequest(
    string? Description = null,
    string? Visibility = null,
    bool? IsArchived = null
);

public record RepositoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string Owner,
    Guid OwnerId,
    string DefaultBranch,
    string Visibility,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

// SSH Key DTOs
public record AddSshKeyRequest(
    string Title,
    string PublicKey
);

public record SshKeyResponse(
    Guid Id,
    string Title,
    string Fingerprint,
    string KeyType,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset CreatedAt
);

// Collaborator DTOs
public record AddCollaboratorRequest(
    string Username,
    string Permission = "read"
);

public record CollaboratorResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string Permission,
    DateTimeOffset CreatedAt
);

// User DTOs
public record UserResponse(
    Guid Id,
    string Username,
    string? Email,
    bool IsAdmin,
    DateTimeOffset CreatedAt
);

// Error response
public record ErrorResponse(
    string Error,
    string? Details = null
);

// Pagination wrapper
public record PagedResponse<T>(
    IEnumerable<T> Data,
    int Skip,
    int Take,
    int? Total = null
);
