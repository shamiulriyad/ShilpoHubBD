using Microsoft.AspNetCore.SignalR;

namespace ShilpoHubBD.Api.Hubs;

// Not [Authorize]d: watching a public live session doesn't require sign-in (per Live Commerce
// requirements). Writes (comments/reactions/purchases) still go through the authorized REST
// endpoints on LiveShoppingController; this hub only fans out the resulting updates.
public class LiveEventHub : Hub
{
    public Task JoinEvent(Guid liveEventId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(liveEventId));

    public Task LeaveEvent(Guid liveEventId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(liveEventId));

    internal static string GroupName(Guid liveEventId) => $"live-event:{liveEventId}";
}
