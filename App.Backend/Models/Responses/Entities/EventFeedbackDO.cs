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

public class EventFeedbackDO : BaseEntityDO<EventFeedback>
{
    public EventFeedbackDO() { }

    public EventFeedbackDO(EventFeedback feedback, Comment comment) : base(feedback)
    {
        EventId = feedback.EventId;
        Rating = feedback.Rating;
        Comment = new(comment);
    }

    /// <summary>
    /// The event this feedback relates to.
    /// </summary>
    [Required]
    public Guid EventId { get; set; }

    /// <summary>
    /// A rating from 0-10 on how to the event was.
    /// </summary>
    [Required]
    public int Rating { get; set; }

    /// <summary>
    /// The comment the user left.
    /// </summary>
    [Required]
    public CommentDO Comment { get; set; } = null!;
}
