using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Pushes real-time updates to viewers of a live event over SignalR. Kept as an abstraction so the
// Application layer has no dependency on the SignalR package; see SignalRLiveEventNotifier.
public interface ILiveEventNotifier
{
    Task NotifyStatusChangedAsync(Guid liveEventId, LiveEventDto liveEvent, CancellationToken cancellationToken);

    Task NotifyCommentAsync(Guid liveEventId, LiveEventCommentDto comment, CancellationToken cancellationToken);

    Task NotifyReactionAsync(Guid liveEventId, List<ReactionSummaryDto> reactionSummary, CancellationToken cancellationToken);

    Task NotifyPurchaseAsync(Guid liveEventId, int purchaseCount, CancellationToken cancellationToken);
}
