using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ShilpoHubBD.Api.Hubs;

[Authorize]
public class LiveClassHub : Hub
{
    public Task JoinClass(Guid liveClassId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(liveClassId));

    public Task LeaveClass(Guid liveClassId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(liveClassId));

    internal static string GroupName(Guid liveClassId) => $"live-class:{liveClassId}";
}
