using Microsoft.AspNetCore.SignalR;
using ShilpoHubBD.Api.Hubs;
using ShilpoHubBD.Application.DTOs.LiveClass;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Realtime;

public class SignalRLiveClassNotifier : ILiveClassNotifier
{
    private readonly IHubContext<LiveClassHub> _hubContext;

    public SignalRLiveClassNotifier(IHubContext<LiveClassHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyStatusChangedAsync(Guid liveClassId, LiveClassDto liveClass, CancellationToken cancellationToken)
        => Group(liveClassId).SendAsync("StatusChanged", liveClass, cancellationToken);

    public Task NotifyParticipantJoinedAsync(Guid liveClassId, LiveClassParticipantDto participant, CancellationToken cancellationToken)
        => Group(liveClassId).SendAsync("ParticipantJoined", participant, cancellationToken);

    public Task NotifyQuestionAskedAsync(Guid liveClassId, LiveClassQuestionDto question, CancellationToken cancellationToken)
        => Group(liveClassId).SendAsync("QuestionAsked", question, cancellationToken);

    public Task NotifyQuestionAnsweredAsync(Guid liveClassId, LiveClassQuestionDto question, CancellationToken cancellationToken)
        => Group(liveClassId).SendAsync("QuestionAnswered", question, cancellationToken);

    private IClientProxy Group(Guid liveClassId) => _hubContext.Clients.Group(LiveClassHub.GroupName(liveClassId));
}
