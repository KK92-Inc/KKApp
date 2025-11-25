/// <summary>
/// Feedback for a given review object.
/// </summary>
[Table("tbl_feedback")]
public class Feedback : BaseEntity
{
    public Feedback()
    {
        ReviewId = Guid.Empty;
        Review = null!;
        
        Comments = string.Empty;
    }

    [Column("review_id")]
    public Guid ReviewId { get; set; }

    [ForeignKey(nameof(ReviewId))]
    public virtual Review Review { get; set; }

    [Column("comments", TypeName = "text")]
    public string Comments { get; set; }
}