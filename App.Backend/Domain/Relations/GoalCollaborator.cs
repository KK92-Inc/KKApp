using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Domain.Relations;

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
