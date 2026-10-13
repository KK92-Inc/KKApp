// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Events;
using App.Backend.Domain.Entities.Users;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

public class EventDO : BaseEntityDO<Event>
{
    public EventDO() { }

    public EventDO(Event @event, IEnumerable<User> users) : base(@event)
    {
        Name = @event.Name;
        Description = @event.Description;
        Thumbnail = @event.Thumbnail;
        Markdown = @event.Markdown;
        Capacity = @event.Capacity;
        Threshold = @event.Threshold;
        StartsAt = @event.StartsAt;
        EndsAt = @event.EndsAt;
        ClosesAt = @event.ClosesAt;
        State = @event.State;
        UserId = @event.UserId;
        Participants = users.Select(u => new UserLightDO(u));
    }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string? Thumbnail { get; set; }

    [Required]
    public string Markdown { get; set; } = null!;

    [Required]
    public int Capacity { get; set; }

    [Required]
    public int? Threshold { get; set; }

    [Required]
    public DateTimeOffset StartsAt { get; set; }

    [Required]
    public DateTimeOffset EndsAt { get; set; }

    [Required]
    public DateTimeOffset ClosesAt { get; set; }

    [Required]
    public EventState State { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public IEnumerable<UserLightDO> Participants { get; set; } = [];
}