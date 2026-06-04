// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using App.Backend.Domain.Entities.Users;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

/// <summary>
/// Generic membership engine. Knows nothing about entity-level rules
/// (capacity, state, git IDs) — callers supply those as arguments or
/// validate them before invoking these methods.
///
/// Works identically for Project, Rubric, Goal, Cursus, UserProject,
/// and Workspace (staff) membership.
/// </summary>
public class MemberService(DatabaseContext ctx) : IMemberService
{
    // ── Query ─────────────────────────────────────────────────────────────────

    public Task<List<Member>> GetAsync(
        MemberEntityType type,
        Guid entityId,
        CancellationToken ct = default)
        => ctx.Members
            .Where(m => m.EntityType == type && m.EntityId == entityId)
            .ToListAsync(ct);

    /// <summary>
    /// EF-translatable predicate — use in Where() clauses when you need to
    /// filter a query by whether a user is an active member of an entity.
    /// </summary>
    public Expression<Func<Member, bool>> IsActiveMember(
        MemberEntityType type, Guid entityId, Guid userId)
        => m => m.EntityType   == type
             && m.EntityId     == entityId
             && m.UserId       == userId
             && m.Role         != MemberRole.Pending
             && m.LeftAt       == null;

    // ── Invite ────────────────────────────────────────────────────────────────

    /// <param name="gitId">
    ///   Pass the entity's git repo ID when the entity has one (Project, Rubric,
    ///   UserProject). Null for entities without a repo (Goal, Cursus, Workspace).
    /// </param>
    /// <param name="maxMembers">
    ///   When provided, the invite is rejected if active member count already
    ///   meets this limit. Null = no cap.
    /// </param>
    public async Task<Member> InviteAsync(
        MemberEntityType type,
        Guid entityId,
        Guid inviterId,
        Guid inviteeId,
        Guid? gitId       = null,
        int? maxMembers   = null,
        CancellationToken token = default)
    {
        if (inviterId == inviteeId)
            throw new ServiceException(400, "You cannot invite yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var members = await GetAsync(type, entityId, ct);

            var inviter = members.FirstOrDefault(m => m.UserId == inviterId);
            if (inviter is null || inviter.Role is not MemberRole.Leader)
                throw new ServiceException(403, "Only a leader can invite members");

            var existing = members.FirstOrDefault(m => m.UserId == inviteeId && m.LeftAt == null);
            if (existing is not null)
            {
                throw existing.Role is MemberRole.Pending
                    ? new ServiceException(409, "User already has a pending invite")
                    : new ServiceException(409, "User is already a member");
            }

            if (maxMembers.HasValue && members.Count(m => m.LeftAt == null) >= maxMembers.Value)
                throw new ServiceException(422, $"Membership is full (max {maxMembers})");

            // Reuse a previously-left row to avoid accumulating duplicates.
            var left = members.FirstOrDefault(m => m.UserId == inviteeId && m.LeftAt is not null);
            Member member;
            if (left is not null)
            {
                left.Role  = MemberRole.Pending;
                left.LeftAt = null;
                left.GitId = gitId;
                ctx.Members.Update(left);
                member = left;
            }
            else
            {
                member = new Member
                {
                    EntityType = type,
                    EntityId   = entityId,
                    GitId      = gitId,
                    UserId     = inviteeId,
                    Role       = MemberRole.Pending,
                };
                await ctx.Members.AddAsync(member, ct);
            }

            await ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return member;
        }, token);
    }

    // ── Uninvite ──────────────────────────────────────────────────────────────

    public async Task<Member> UninviteAsync(
        MemberEntityType type,
        Guid entityId,
        Guid inviterId,
        Guid inviteeId,
        CancellationToken token = default)
    {
        if (inviterId == inviteeId)
            throw new ServiceException(400, "You cannot uninvite yourself");

        var members = await GetAsync(type, entityId, token);

        var inviter = members.FirstOrDefault(m => m.UserId == inviterId);
        if (inviter is null || inviter.Role is not MemberRole.Leader)
            throw new ServiceException(403, "Only a leader can cancel invites");

        var target = members.FirstOrDefault(m => m.UserId == inviteeId)
            ?? throw new ServiceException(404, "User was never invited");

        if (target.Role is not MemberRole.Pending)
            throw new ServiceException(409, "User is already active — use kick instead");

        ctx.Members.Remove(target);
        await ctx.SaveChangesAsync(token);
        return target;
    }

    // ── Accept / Decline ──────────────────────────────────────────────────────

    public async Task<Member> AcceptAsync(
        MemberEntityType type,
        Guid entityId,
        Guid userId,
        CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.Members.FirstOrDefaultAsync(m =>
                    m.EntityType == type   &&
                    m.EntityId   == entityId &&
                    m.UserId     == userId &&
                    m.Role       == MemberRole.Pending, ct)
                ?? throw new ServiceException(404, "No pending invite found");

            member.Role = MemberRole.Member;
            ctx.Members.Update(member);
            await ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return member;
        }, token);
    }

    public async Task<Member> DeclineAsync(
        MemberEntityType type,
        Guid entityId,
        Guid userId,
        CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.Members.FirstOrDefaultAsync(m =>
                    m.EntityType == type   &&
                    m.EntityId   == entityId &&
                    m.UserId     == userId &&
                    m.Role       == MemberRole.Pending, ct)
                ?? throw new ServiceException(404, "No pending invite found");

            ctx.Members.Remove(member);
            await ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return member;
        }, token);
    }

    // ── Leadership ────────────────────────────────────────────────────────────

    public async Task TransferLeadershipAsync(
        MemberEntityType type,
        Guid entityId,
        Guid currentLeaderId,
        Guid newLeaderId,
        CancellationToken token = default)
    {
        if (currentLeaderId == newLeaderId)
            throw new ServiceException(400, "Cannot transfer leadership to yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async ct =>
        {
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var members = await GetAsync(type, entityId, ct);

            var leader = members.FirstOrDefault(m => m.UserId == currentLeaderId);
            if (leader is null || leader.Role is not MemberRole.Leader)
                throw new ServiceException(403, "Only the leader can transfer leadership");

            var target = members.FirstOrDefault(m => m.UserId == newLeaderId);
            if (target is null || target.LeftAt is not null)
                throw new ServiceException(404, "Target user is not an active member");
            if (target.Role is MemberRole.Pending)
                throw new ServiceException(422, "Cannot transfer leadership to a pending member");

            leader.Role = MemberRole.Member;
            target.Role = MemberRole.Leader;
            ctx.Members.UpdateRange(leader, target);

            await ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }, token);
    }

    // ── Leave / Kick ──────────────────────────────────────────────────────────

    public async Task LeaveAsync(
        MemberEntityType type,
        Guid entityId,
        Guid userId,
        CancellationToken token = default)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async ct =>
        {
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.Members.FirstOrDefaultAsync(m =>
                    m.EntityType == type     &&
                    m.EntityId   == entityId &&
                    m.UserId     == userId   &&
                    m.LeftAt     == null, ct)
                ?? throw new ServiceException(404, "Not a member of this entity");

            if (member.Role is MemberRole.Leader)
                throw new ServiceException(422, "Leaders cannot leave — transfer leadership first");
            if (member.Role is MemberRole.Pending)
                throw new ServiceException(422, "Use decline to reject a pending invite");

            member.LeftAt = DateTimeOffset.UtcNow;
            ctx.Members.Update(member);
            await ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }, token);
    }

    public async Task KickAsync(
        MemberEntityType type,
        Guid entityId,
        Guid leaderId,
        Guid memberId,
        CancellationToken token = default)
    {
        if (leaderId == memberId)
            throw new ServiceException(400, "You cannot kick yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async ct =>
        {
            await using var tx = await ctx.Database.BeginTransactionAsync(ct);

            var members = await GetAsync(type, entityId, ct);

            var leader = members.FirstOrDefault(m => m.UserId == leaderId);
            if (leader is null || leader.Role is not MemberRole.Leader)
                throw new ServiceException(403, "Only the leader can kick members");

            var target = members.FirstOrDefault(m => m.UserId == memberId);
            if (target is null || target.LeftAt is not null)
                throw new ServiceException(404, "User is not an active member");

            // Pending members are just removed; active members get a left_at timestamp
            // so their history is preserved.
            if (target.Role is MemberRole.Pending)
                ctx.Members.Remove(target);
            else
            {
                target.LeftAt = DateTimeOffset.UtcNow;
                ctx.Members.Update(target);
            }

            await ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }, token);
    }
}