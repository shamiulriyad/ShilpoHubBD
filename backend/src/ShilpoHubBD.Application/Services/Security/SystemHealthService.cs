using System.Diagnostics;
using System.Runtime.InteropServices;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Security;

public class SystemHealthService : ISystemHealthService
{
    private readonly ISystemHealthRepository _repository;

    public SystemHealthService(ISystemHealthRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemHealthDto> GetHealthAsync(CancellationToken cancellationToken)
    {
        var connected = await _repository.CanConnectAsync(cancellationToken);
        var counts = connected
            ? await _repository.GetCountsAsync(cancellationToken)
            : (Users: 0, Orders: 0, Products: 0);

        var process = Process.GetCurrentProcess();

        return new SystemHealthDto
        {
            DatabaseConnected = connected,
            Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
            UserCount = counts.Users,
            OrderCount = counts.Orders,
            ProductCount = counts.Products,
            WorkingSetBytes = process.WorkingSet64,
            MachineName = Environment.MachineName,
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            GeneratedAt = DateTime.UtcNow,
        };
    }
}
