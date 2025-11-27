using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Backend.Domain.Entities;
using App.Backend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Backend.Domain.Relations;

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
