// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Events;
using App.Backend.Domain.Enums;

// ============================================================================

namespace App.Backend.Models.Responses.Entities;

public class EventDO(Event @event) : BaseEntityDO<Event>(@event)
{
    [Required]
    public string Name { get; set; } = @event.Name;

    [Required]
    public string Description { get; set; } = @event.Description;

    [Required]
    public string? Thumbnail { get; set; } = @event.Thumbnail;

    [Required]
    public string Markdown { get; set; } = @event.Markdown;

    [Required]
    public int Capacity { get; set; } = @event.Capacity;

    [Required]
    public int? Threshold { get; set; } = @event.Threshold;

    [Required]
    public DateTimeOffset StartsAt { get; set; } = @event.StartsAt;

    [Required]
    public DateTimeOffset EndsAt { get; set; } = @event.EndsAt;

    [Required]
    public DateTimeOffset? ClosesAt { get; set; } = @event.ClosesAt;

    [Required]
    public EventState State { get; set; } = @event.State;

    [Required]
    public Guid UserId { get; set; } = @event.UserId;

    public static implicit operator EventDO?(Event? @event) => @event is null ? null : new(@event);
}