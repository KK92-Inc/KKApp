using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Domain.Entities;
using Backend.API.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Domain.Relations;

[Table("rel_cursuscollaborator")]
[PrimaryKey(nameof(UserId), nameof(CursusId))]
public class CursusCollaborator
{
    [Column(Order = 0)]
    public Guid UserId { get; set; }
    [Column(Order = 1)]
    public Guid CursusId { get; set; }

    public virtual User User { get; set; }
    public virtual Cursus Cursus { get; set; }
}