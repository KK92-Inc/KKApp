using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Domain.Entities;
using Backend.API.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Domain.Relations;

[Table("rel_goalcollaborator")]
[PrimaryKey(nameof(UserId), nameof(GoalId))]
public class GoalCollaborator
{
    [Column(Order = 0)]
    public Guid UserId { get; set; }
    [Column(Order = 1)]
    public Guid GoalId { get; set; }

    public virtual User User { get; set; }
    public virtual Goal Goal { get; set; }
}