namespace App.Backend.Database;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}