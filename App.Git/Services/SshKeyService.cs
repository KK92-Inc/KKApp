// ============================================================================
// Copyright (c) 2025 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Git.Data;
using App.Git.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Git.Services;

/// <summary>
/// Service for SSH key operations.
/// </summary>
public class SshKeyService
{
    private readonly GitDbContext _db;
    private readonly ILogger<SshKeyService> _logger;

    public SshKeyService(GitDbContext db, ILogger<SshKeyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Adds a new SSH key for a user.
    /// </summary>
    public async Task<SshKey> AddKeyAsync(Guid userId, string title, string publicKey)
    {
        // Parse and validate the key
        var (keyType, fingerprint) = ParsePublicKey(publicKey);

        // Check for duplicate fingerprint
        var existing = await _db.SshKeys.FirstOrDefaultAsync(k => k.Fingerprint == fingerprint);
        if (existing != null)
        {
            throw new InvalidOperationException("This SSH key is already registered");
        }

        var sshKey = new SshKey
        {
            UserId = userId,
            Title = title,
            PublicKey = publicKey.Trim(),
            KeyType = keyType,
            Fingerprint = fingerprint
        };

        _db.SshKeys.Add(sshKey);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Added SSH key {KeyId} for user {UserId}", sshKey.Id, userId);
        return sshKey;
    }

    /// <summary>
    /// Removes an SSH key.
    /// </summary>
    public async Task<bool> RemoveKeyAsync(Guid userId, Guid keyId)
    {
        var key = await _db.SshKeys.FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);
        if (key == null)
        {
            return false;
        }

        _db.SshKeys.Remove(key);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Removed SSH key {KeyId} for user {UserId}", keyId, userId);
        return true;
    }

    /// <summary>
    /// Gets all SSH keys for a user.
    /// </summary>
    public async Task<List<SshKey>> GetKeysAsync(Guid userId)
    {
        return await _db.SshKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a user by SSH key fingerprint (for SSH authentication).
    /// </summary>
    public async Task<User?> FindUserByFingerprintAsync(string fingerprint)
    {
        var key = await _db.SshKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Fingerprint == fingerprint);

        if (key != null)
        {
            key.LastUsedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        return key?.User;
    }

    /// <summary>
    /// Parses an SSH public key and extracts type and fingerprint.
    /// </summary>
    private static (string keyType, string fingerprint) ParsePublicKey(string publicKey)
    {
        var parts = publicKey.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            throw new ArgumentException("Invalid SSH public key format");
        }

        var keyType = parts[0];
        var keyData = parts[1];

        // Validate key type
        var validTypes = new[] { "ssh-rsa", "ssh-ed25519", "ecdsa-sha2-nistp256", "ecdsa-sha2-nistp384", "ecdsa-sha2-nistp521" };
        if (!validTypes.Contains(keyType))
        {
            throw new ArgumentException($"Unsupported key type: {keyType}");
        }

        // Calculate SHA256 fingerprint
        var keyBytes = Convert.FromBase64String(keyData);
        var hash = SHA256.HashData(keyBytes);
        var fingerprint = $"SHA256:{Convert.ToBase64String(hash).TrimEnd('=')}";

        return (keyType, fingerprint);
    }
}
