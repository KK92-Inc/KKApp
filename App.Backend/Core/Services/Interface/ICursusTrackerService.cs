namespace App.Backend.Core.Services.Interface;

public interface ICursusSnapshotTracker
{
    Task AdvanceTrackAsync(Guid userId, Guid cursusId, Guid userCursusId, CancellationToken token = default);
}
