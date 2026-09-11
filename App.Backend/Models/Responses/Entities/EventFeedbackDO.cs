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

public class EventFeedbackDO(EventFeedback feedback, Comment comment) : BaseEntityDO<EventFeedback>(feedback)
{
    /// <summary>
    /// The event this feedback relates to.
    /// </summary>
    [Required]
    public Guid EventId { get; set; } = feedback.EventId;

    /// <summary>
    /// A rating from 0-10 on how to the event was.
    /// </summary>
    [Required]
    public int Rating { get; set; } = feedback.Rating;

    /// <summary>
    /// The comment the user left.
    /// </summary>
    [Required]
    public CommentDO Comment { get; set; } = new(comment);
}
