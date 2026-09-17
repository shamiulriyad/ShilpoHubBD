namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ISystemHealthRepository
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
    Task<(int Users, int Orders, int Products)> GetCountsAsync(CancellationToken cancellationToken);
}
