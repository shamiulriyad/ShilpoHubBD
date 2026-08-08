using ShilpoHubBD.Application.DTOs.Messaging;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMessageNotifier
{
    Task NotifyMessageAsync(Guid recipientUserId, MessageDto message, CancellationToken cancellationToken);
}
