using Microsoft.AspNetCore.SignalR;
using ShilpoHubBD.Api.Hubs;
using ShilpoHubBD.Application.DTOs.LiveShopping;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Realtime;

public class SignalRLiveEventNotifier : ILiveEventNotifier
{
    private readonly IHubContext<LiveEventHub> _hubContext;

    public SignalRLiveEventNotifier(IHubContext<LiveEventHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyStatusChangedAsync(Guid liveEventId, LiveEventDto liveEvent, CancellationToken cancellationToken)
        => Group(liveEventId).SendAsync("StatusChanged", liveEvent, cancellationToken);

    public Task NotifyCommentAsync(Guid liveEventId, LiveEventCommentDto comment, CancellationToken cancellationToken)
        => Group(liveEventId).SendAsync("CommentAdded", comment, cancellationToken);

    public Task NotifyReactionAsync(Guid liveEventId, List<ReactionSummaryDto> reactionSummary, CancellationToken cancellationToken)
        => Group(liveEventId).SendAsync("ReactionsUpdated", reactionSummary, cancellationToken);

    public Task NotifyPurchaseAsync(Guid liveEventId, int purchaseCount, CancellationToken cancellationToken)
        => Group(liveEventId).SendAsync("PurchaseMade", purchaseCount, cancellationToken);

    private IClientProxy Group(Guid liveEventId) => _hubContext.Clients.Group(LiveEventHub.GroupName(liveEventId));
}
