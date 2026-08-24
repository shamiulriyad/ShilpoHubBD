using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.LiveClass;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ILiveClassService
{
    Task<LiveClassDto> CreateAsync(Guid userId, CreateLiveClassRequest request, CancellationToken cancellationToken);

    Task<LiveClassDto> UpdateAsync(Guid userId, Guid liveClassId, UpdateLiveClassRequest request, CancellationToken cancellationToken);

    Task<LiveClassDto> StartAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task<LiveClassDto> EndAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task<LiveClassDto> CancelAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task<LiveClassDto> GetByIdAsync(Guid liveClassId, CancellationToken cancellationToken);

    Task<PagedResult<LiveClassListItemDto>> GetPagedAsync(LiveClassQueryParameters query, CancellationToken cancellationToken);

    Task<List<LiveClassListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken);

    Task<List<LiveClassListItemDto>> GetRegisteredAsync(Guid userId, CancellationToken cancellationToken);

    Task<LiveClassParticipantDto> RegisterAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task JoinAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task LeaveAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task<List<LiveClassAttendanceDto>> GetAttendanceAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken);

    Task<LiveClassQuestionDto> AskQuestionAsync(Guid userId, Guid liveClassId, AskQuestionRequest request, CancellationToken cancellationToken);

    Task<LiveClassQuestionDto> AnswerQuestionAsync(
        Guid userId, Guid liveClassId, Guid questionId, AnswerQuestionRequest request, CancellationToken cancellationToken);
}
