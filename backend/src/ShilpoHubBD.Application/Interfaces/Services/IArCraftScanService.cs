using ShilpoHubBD.Application.DTOs.ArVr;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IArCraftScanService
{
    Task<ArCraftScanResultDto> ScanAsync(Guid? scannedByUserId, ArCraftScanRequest request, CancellationToken cancellationToken);
}
