using ShilpoHubBD.Application.DTOs.LiveClass;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Pushes real-time updates to viewers of a live class over SignalR. Kept as an abstraction so the
// Application layer has no dependency on the SignalR package; see SignalRLiveClassNotifier.
public interface ILiveClassNotifier
{
    Task NotifyStatusChangedAsync(Guid liveClassId, LiveClassDto liveClass, CancellationToken cancellationToken);

    Task NotifyParticipantJoinedAsync(Guid liveClassId, LiveClassParticipantDto participant, CancellationToken cancellationToken);

    Task NotifyQuestionAskedAsync(Guid liveClassId, LiveClassQuestionDto question, CancellationToken cancellationToken);

    Task NotifyQuestionAnsweredAsync(Guid liveClassId, LiveClassQuestionDto question, CancellationToken cancellationToken);
}
