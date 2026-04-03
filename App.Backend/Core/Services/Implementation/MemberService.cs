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

public class MemberService(DatabaseContext ctx) : IMemberService
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads all Member rows for a given UserProject in one query.
    /// Replaces the old Include(p => p.Members) pattern.
    /// </summary>
    private Task<List<Member>> LoadProjectMembersAsync(Guid userProjectId, CancellationToken ct)
        => ctx.Members
            .Where(m => m.EntityType == MemberEntityType.UserProject
                     && m.EntityId == userProjectId)
            .ToListAsync(ct);

    // -------------------------------------------------------------------------
    // Invite
    // -------------------------------------------------------------------------

    // Add to MemberService

    public Task<List<Member>> GetProjectMembersAsync(Guid userProjectId, CancellationToken token)
        => LoadProjectMembersAsync(userProjectId, token);

    // Returns a composable LINQ expression EF can translate to SQL.
    // This replaces the inline up.Members.Any(...) lambda that no longer compiles.
    public Expression<Func<UserProject, bool>> HasActiveMember(Guid userProjectId, Guid userId)
        => up => ctx.Members.Any(m =>
            m.EntityType == MemberEntityType.UserProject &&
            m.EntityId == userProjectId &&
            m.UserId == userId &&
            m.Role != MemberRole.Pending &&
            m.LeftAt == null);

    public async Task<Member> InviteToProjectAsync(
        Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        if (inviterId == inviteeId)
            throw new ServiceException(400, "You cannot invite yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            // No longer Include(p => p.Members) — members live in tbl_members now
            var up = await ctx.UserProjects
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == userProjectId, ct)
                ?? throw new ServiceException(404, "Project session not found");

            if (up.State is not EntityObjectState.Active)
                throw new ServiceException(422, "Project session is not active");

            var members = await LoadProjectMembersAsync(userProjectId, ct);

            var inviter = members.FirstOrDefault(m => m.UserId == inviterId);
            if (inviter is null || inviter.Role is not MemberRole.Leader)
                throw new ServiceException(403, "Only the session leader can invite members");

            var existing = members.FirstOrDefault(m => m.UserId == inviteeId && m.LeftAt == null);
            if (existing is not null)
            {
                if (existing.Role is MemberRole.Pending)
                    throw new ServiceException(409, "User already has a pending invite");
                throw new ServiceException(409, "User is already a member of this session");
            }

            var activeCount = members.Count(m => m.LeftAt == null);
            if (activeCount >= up.MaxMembers)
                throw new ServiceException(422, $"Project session is full (max {up.MaxMembers} members)");

            // Reuse a previously-left row to avoid accumulating duplicates
            Member member;
            var left = members.FirstOrDefault(m => m.UserId == inviteeId && m.LeftAt is not null);
            if (left is not null)
            {
                left.Role = MemberRole.Pending;
                left.LeftAt = null;
                ctx.Members.Update(left);
                member = left;
            }
            else
            {
                member = new Member
                {
                    EntityType = MemberEntityType.UserProject,
                    EntityId = userProjectId,
                    GitId = up.GitInfoId,   // null if the session has no repo yet
                    UserId = inviteeId,
                    Role = MemberRole.Pending,
                };
                await ctx.Members.AddAsync(member, ct);
            }

            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = inviterId,
                UserProjectId = userProjectId,
                Type = UserProjectTransactionVariant.MemberInvited,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return member;
        }, token);
    }

    // -------------------------------------------------------------------------
    // Uninvite
    // -------------------------------------------------------------------------

    public async Task<Member> UninviteFromProjectAsync(
        Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        if (inviterId == inviteeId)
            throw new ServiceException(400, "You cannot uninvite yourself");

        var up = await ctx.UserProjects
            .FirstOrDefaultAsync(p => p.Id == userProjectId, token)
            ?? throw new ServiceException(404, "Project session not found");

        if (up.State is not EntityObjectState.Active)
            throw new ServiceException(422, "Project session is not active");

        var members = await LoadProjectMembersAsync(userProjectId, token);

        var inviter = members.FirstOrDefault(m => m.UserId == inviterId);
        if (inviter is null || inviter.Role is not MemberRole.Leader)
            throw new ServiceException(403, "Only the session leader can cancel invites");

        var existing = members.FirstOrDefault(m => m.UserId == inviteeId)
            ?? throw new ServiceException(409, "User was never invited to this session");

        if (existing.Role is not MemberRole.Pending)
            throw new ServiceException(409, "User is already an active member, use kick instead");

        ctx.Members.Remove(existing);
        await ctx.SaveChangesAsync(token);
        return existing;
    }

    // -------------------------------------------------------------------------
    // Accept / Decline invite
    // -------------------------------------------------------------------------

    public async Task<Member> AcceptInviteAsync(Guid userId, Guid userProjectId, CancellationToken token)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.Members
                .FirstOrDefaultAsync(m =>
                    m.EntityType == MemberEntityType.UserProject &&
                    m.EntityId == userProjectId &&
                    m.UserId == userId &&
                    m.Role == MemberRole.Pending, ct)
                ?? throw new ServiceException(404, "No pending invite found");

            var state = await ctx.UserProjects
                .Where(p => p.Id == userProjectId)
                .Select(p => p.State)
                .FirstAsync(ct);

            if (state is not EntityObjectState.Active)
                throw new ServiceException(422, "Project session is no longer active");

            member.Role = MemberRole.Member;
            ctx.Members.Update(member);

            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = userId,
                UserProjectId = userProjectId,
                Type = UserProjectTransactionVariant.MemberAccepted,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return member;
        }, token);
    }

    public async Task DeclineInviteAsync(Guid userId, Guid userProjectId, CancellationToken token)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.Members
                .FirstOrDefaultAsync(m =>
                    m.EntityType == MemberEntityType.UserProject &&
                    m.EntityId == userProjectId &&
                    m.UserId == userId &&
                    m.Role == MemberRole.Pending, ct)
                ?? throw new ServiceException(404, "No pending invite found");

            ctx.Members.Remove(member);

            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = userId,
                UserProjectId = userProjectId,
                Type = UserProjectTransactionVariant.MemberDeclined,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, token);
    }

    // -------------------------------------------------------------------------
    // Leadership transfer
    // -------------------------------------------------------------------------

    public async Task TransferLeadershipAsync(
        Guid currentLeaderId, Guid newLeaderId, Guid userProjectId, CancellationToken token)
    {
        if (currentLeaderId == newLeaderId)
            throw new ServiceException(400, "Cannot transfer leadership to yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var up = await ctx.UserProjects
                .FirstOrDefaultAsync(p => p.Id == userProjectId, ct)
                ?? throw new ServiceException(404, "Project session not found");

            if (up.State is not EntityObjectState.Active)
                throw new ServiceException(422, "Project session is not active");

            var members = await LoadProjectMembersAsync(userProjectId, ct);

            var leader = members.FirstOrDefault(m => m.UserId == currentLeaderId);
            if (leader is null || leader.Role is not MemberRole.Leader)
                throw new ServiceException(403, "Only the session leader can transfer leadership");

            var target = members.FirstOrDefault(m => m.UserId == newLeaderId);
            if (target is null || target.LeftAt is not null)
                throw new ServiceException(404, "Target user is not an active member of this session");
            if (target.Role is MemberRole.Pending)
                throw new ServiceException(422, "Cannot transfer leadership to a pending member");

            leader.Role = MemberRole.Member;
            target.Role = MemberRole.Leader;

            ctx.Members.UpdateRange(leader, target);

            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = currentLeaderId,
                UserProjectId = userProjectId,
                Type = UserProjectTransactionVariant.LeadershipTransferred,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, token);
    }

    // -------------------------------------------------------------------------
    // Leave / Kick
    // -------------------------------------------------------------------------

    public async Task LeaveProjectAsync(Guid userId, Guid userProjectId, CancellationToken token)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.Members
                .FirstOrDefaultAsync(m =>
                    m.EntityType == MemberEntityType.UserProject &&
                    m.EntityId == userProjectId &&
                    m.UserId == userId &&
                    m.LeftAt == null, ct)
                ?? throw new ServiceException(404, "Not a member of this session");

            if (member.Role is MemberRole.Leader)
                throw new ServiceException(422,
                    "Leaders cannot leave — transfer leadership first or unsubscribe from the project");

            if (member.Role is MemberRole.Pending)
                throw new ServiceException(422,
                    "Use the decline endpoint to decline a pending invite");

            member.LeftAt = DateTimeOffset.UtcNow;
            ctx.Members.Update(member);

            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = userId,
                UserProjectId = userProjectId,
                Type = UserProjectTransactionVariant.MemberLeft,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, token);
    }

    public async Task KickMemberAsync(Guid leaderId, Guid memberId, Guid userProjectId, CancellationToken token)
    {
        if (leaderId == memberId)
            throw new ServiceException(400, "You cannot kick yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var up = await ctx.UserProjects
                .FirstOrDefaultAsync(p => p.Id == userProjectId, ct)
                ?? throw new ServiceException(404, "Project session not found");

            if (up.State is not EntityObjectState.Active)
                throw new ServiceException(422, "Project session is not active");

            var members = await LoadProjectMembersAsync(userProjectId, ct);

            var leader = members.FirstOrDefault(m => m.UserId == leaderId);
            if (leader is null || leader.Role is not MemberRole.Leader)
                throw new ServiceException(403, "Only the session leader can kick members");

            var target = members.FirstOrDefault(m => m.UserId == memberId);
            if (target is null || target.LeftAt is not null)
                throw new ServiceException(404, "User is not an active member of this session");

            if (target.Role is MemberRole.Pending)
                ctx.Members.Remove(target);          // pending = just remove, no left_at
            else
            {
                target.LeftAt = DateTimeOffset.UtcNow;
                ctx.Members.Update(target);
            }

            await ctx.UserProjectTransactions.AddAsync(new()
            {
                UserId = leaderId,
                UserProjectId = userProjectId,
                Type = UserProjectTransactionVariant.MemberKicked,
            }, ct);

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, token);
    }
}