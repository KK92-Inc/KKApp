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
public class MemberService(DatabaseContext context, TimeProvider time) : BaseService<Member>(context), IMemberService
{
    private readonly DatabaseContext ctx = context;

    [Obsolete("Use Invite instead as it tracks other things.")]
    public override Task<Member> CreateAsync(Member entity, CancellationToken token = default)
    {
        return base.CreateAsync(entity, token);
    }

    [Obsolete("Use Kick or Cancel instead as it tracks other things.")]
    public override Task DeleteAsync(Member entity, CancellationToken token = default)
    {
        return base.DeleteAsync(entity, token);
    }

    public async Task<Member> AcceptAsync(Guid memberId, CancellationToken token = default)
    {
        var member = await ctx.Members.FirstOrDefaultAsync(
            m => m.Id == memberId
            && m.Role == MemberRole.Pending,
        token) ?? throw new ServiceException(404, "No pending invite found");

        member.Role = MemberRole.Member;
        member = ctx.Members.Update(member).Entity;
        await ctx.SaveChangesAsync(token);
        return member;
    }

    public async Task<Member> DeclineAsync(Guid memberId, CancellationToken token = default)
    {
        var member = await ctx.Members.FirstOrDefaultAsync(
            m => m.Id == memberId
            && m.Role == MemberRole.Pending,
        token) ?? throw new ServiceException(404, "No pending invite found");

        ctx.Members.Remove(member);
        await ctx.SaveChangesAsync(token);
        return member;
    }

    public async Task<Member?> FindByEntityAndUserId(Guid entityId, Guid userId, CancellationToken token = default)
    {
        return await ctx.Members.FirstOrDefaultAsync(
            m => m.UserId == userId
            && m.EntityId == entityId,
        token);
    }

    public async Task<Member> InviteAsync(Guid entityId, Guid userId, Guid? gitId, int? max, CancellationToken token = default)
    {
        // Check existing membership
        var existing = await FindByEntityAndUserId(entityId, userId, token);
        if (existing is not null && existing.UserId == userId)
            throw new ServiceException(409, "User is already a member or pending an invite");

        // Check if there is space left to join
        var members = ctx.Members.AsNoTracking().Where(m => m.EntityId == entityId);
        var count = await members.CountAsync(token);
        if (max.HasValue && count >= max.Value)
            throw new ServiceException(422, $"Membership is full (max {max})");

        // Implement the membership
        Member member;  // Reuse a previously-left row to avoid accumulating duplicates.
        var left = members.FirstOrDefault(m => m.UserId == userId && m.LeftAt != null);
        if (left is not null)
        {
            left.Role = MemberRole.Pending;
            left.LeftAt = null;
            left.GitId = gitId;
            ctx.Members.Update(left);
            member = left;
        }
        else
        {
            var leader = await members
                .AsNoTracking()
                .Where(m => m.Role == MemberRole.Leader)
                .FirstOrDefaultAsync(token)
            ?? throw new ServiceException(500, "Entity has no leader, corrupted.");

            member = new Member
            {
                // NOTE(W2): We inherit the type from leader.
                // Wonder if this will lead to bugs... but shouldn't.
                EntityType = leader.EntityType,
                EntityId = entityId,
                GitId = gitId,
                UserId = userId,
                Role = MemberRole.Pending,
            };
            await ctx.Members.AddAsync(member, token);
        }

        await ctx.SaveChangesAsync(token);
        return member;
    }


    public async Task<Member> KickAsync(Guid memberId, CancellationToken token = default)
    {
        var member = await ctx.Members.FirstOrDefaultAsync(m => m.Id == memberId, token)
            ?? throw new ServiceException(404, "Membership does not exist");

        if (member.Role is MemberRole.Pending)
            return await UnInviteAsync(member.EntityId, member.UserId, token);
        else
        {
            member.LeftAt = time.GetUtcNow();
            member = ctx.Members.Update(member).Entity;
        }

        await ctx.SaveChangesAsync(token);
        return member;
    }

    public async Task<Member> LeaveAsync(Guid memberId, CancellationToken token = default)
    {
        var member = await ctx.Members.FirstOrDefaultAsync(m => m.Id == memberId, token)
            ?? throw new ServiceException(404, "You're not a member");

        ServiceException.ThrowIf(member.Role is MemberRole.Leader, "Unable to leave as leader, transfer role first");
        ServiceException.ThrowIf(member.Role is MemberRole.Pending, "You're not a member");

        member.LeftAt = time.GetUtcNow();
        member = ctx.Members.Update(member).Entity;
        await ctx.SaveChangesAsync(token);
        return member;
    }

    public Task<Member> SetRoleAsync(Guid memberId, MemberRole role, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Member> UnInviteAsync(Guid entityId, Guid userId, CancellationToken token = default)
    {
        // Check existing membership
        var existing = await FindByEntityAndUserId(entityId, userId, token)
            ?? throw new ServiceException(404, "No pending invite to revoke");

        // ServiceException.ThrowIf(existing.UserId == userId, "You cannot uninvite yourself");
        ServiceException.ThrowIf(existing.Role is not MemberRole.Pending, "User has already accepted, kick them instead");

        ctx.Members.Remove(existing);
        await ctx.SaveChangesAsync(token);
        return existing;
    }
}