// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using App.Backend.Database;
using App.Backend.Core.Services.Interface;
using App.Backend.Domain.Entities.Projects;
using App.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// ============================================================================

namespace App.Backend.Core.Services.Implementation;

public class InviteService(DatabaseContext ctx) : IInviteService
{
    public async Task<UserProjectMember> InviteToProjectAsync(Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        if (inviterId == inviteeId)
            throw new ServiceException(400, "You cannot invite yourself");

        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            // Load the session with members and the project template (for MaxMembers)
            var up = await ctx.UserProjects
                .Include(p => p.Members)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == userProjectId, ct)
                ?? throw new ServiceException(404, "Project session not found");

            if (up.State is not EntityObjectState.Active)
                throw new ServiceException("Project session is not active");

            // Only the leader can invite
            var inviter = up.Members.FirstOrDefault(m => m.UserId == inviterId);
            if (inviter is null || inviter.Role is not UserProjectRole.Leader)
                throw new ServiceException(403, "Only the session leader can invite members");

            // Check if the invitee is already a member or has a pending invite
            var existing = up.Members.FirstOrDefault(m => m.UserId == inviteeId);
            if (existing is not null)
            {
                if (existing.Role is UserProjectRole.Pending)
                    throw new ServiceException(409, "User already has a pending invite");
                throw new ServiceException(409, "User is already a member of this session");
            }

            // Count active members (non-pending, non-left) + pending invites against max
            var count = up.Members.Count(m => m.LeftAt == null);
            if (count >= 5) // TODO: Define on project or define on env ?
                throw new ServiceException(422, $"Project session is full (max {up.Project.MaxMembers} members)");

            // Add the pending member
            var member = new UserProjectMember
            {
                UserProjectId = userProjectId,
                UserId = inviteeId,
                Role = UserProjectRole.Pending,
            };

            await ctx.UserProjectMembers.AddAsync(member, ct);
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

    public async Task<UserProjectMember> UninviteFromProjectAsync(Guid inviterId, Guid inviteeId, Guid userProjectId, CancellationToken token)
    {
        if (inviterId == inviteeId)
            throw new ServiceException(400, "You cannot uninvite yourself");

        var up = await ctx.UserProjects
            .Include(p => p.Members)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(p => p.Id == userProjectId, token)
            ?? throw new ServiceException(404, "Project session not found");

        if (up.State is not EntityObjectState.Active)
            throw new ServiceException("Project session is not active");

        // Only the leader can cancel invites
        var inviter = up.Members.FirstOrDefault(m => m.UserId == inviterId);
        if (inviter is null || inviter.Role is not UserProjectRole.Leader)
            throw new ServiceException(403, "Only the session leader can cancel invites");

        // Check if the invitee is a member or has a pending invite
        var existing = up.Members.FirstOrDefault(m => m.UserId == inviteeId)
            ?? throw new ServiceException(409, "User was never invited to be a member of this session");

        if (existing.Role is not UserProjectRole.Pending)
            throw new ServiceException(409, "User is a member");

        ctx.UserProjectMembers.Remove(existing);
        await ctx.SaveChangesAsync(token);
        return existing;
    }

    public async Task<UserProjectMember> AcceptInviteAsync(Guid userId, Guid userProjectId, CancellationToken token)
    {
        var strategy = ctx.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

            var member = await ctx.UserProjectMembers
                .FirstOrDefaultAsync(m =>
                    m.UserProjectId == userProjectId &&
                    m.UserId == userId &&
                    m.Role == UserProjectRole.Pending, ct)
                ?? throw new ServiceException(404, "No pending invite found");

            // Verify the session is still active
            var state = await ctx.UserProjects
                .Where(p => p.Id == userProjectId)
                .Select(p => p.State)
                .FirstAsync(ct);

            if (state is not EntityObjectState.Active)
                throw new ServiceException(422, "Project session is no longer active");

            member.Role = UserProjectRole.Member;
            ctx.UserProjectMembers.Update(member);

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

            var member = await ctx.UserProjectMembers
                .FirstOrDefaultAsync(m =>
                    m.UserProjectId == userProjectId &&
                    m.UserId == userId &&
                    m.Role == UserProjectRole.Pending, ct)
                ?? throw new ServiceException(404, "No pending invite found");

            ctx.UserProjectMembers.Remove(member);

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
}
