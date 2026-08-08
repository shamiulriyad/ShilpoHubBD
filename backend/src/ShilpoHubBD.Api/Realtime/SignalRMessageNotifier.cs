using Microsoft.AspNetCore.SignalR;
using ShilpoHubBD.Api.Hubs;
using ShilpoHubBD.Application.DTOs.Messaging;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Realtime;

public class SignalRMessageNotifier : IMessageNotifier
{
    private readonly IHubContext<MessagingHub> _hubContext;

    public SignalRMessageNotifier(IHubContext<MessagingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyMessageAsync(Guid recipientUserId, MessageDto message, CancellationToken cancellationToken)
        => _hubContext.Clients
            .Group(MessagingHub.GroupName(recipientUserId))
            .SendAsync("ReceiveMessage", message, cancellationToken);
}
