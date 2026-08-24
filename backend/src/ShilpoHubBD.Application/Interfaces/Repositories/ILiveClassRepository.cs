using ShilpoHubBD.Application.DTOs.LiveClass;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ILiveClassRepository
{
    Task<LiveClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<LiveClass> Items, int TotalCount)> GetPagedAsync(LiveClassQueryParameters query, CancellationToken cancellationToken);
    Task<List<LiveClass>> GetByInstructorAsync(Guid instructorUserId, CancellationToken cancellationToken);
    Task AddAsync(LiveClass liveClass, CancellationToken cancellationToken);
    void Remove(LiveClass liveClass);

    Task<LiveClassParticipant?> GetParticipantAsync(Guid liveClassId, Guid userId, CancellationToken cancellationToken);
    Task AddParticipantAsync(LiveClassParticipant participant, CancellationToken cancellationToken);
    Task<List<LiveClass>> GetMyRegisteredAsync(Guid userId, CancellationToken cancellationToken);

    Task<LiveClassQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task AddQuestionAsync(LiveClassQuestion question, CancellationToken cancellationToken);

    Task<LiveClassAttendance?> GetOpenAttendanceAsync(Guid liveClassId, Guid userId, CancellationToken cancellationToken);
    Task AddAttendanceAsync(LiveClassAttendance attendance, CancellationToken cancellationToken);
    Task<List<LiveClassAttendance>> GetAttendanceAsync(Guid liveClassId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
