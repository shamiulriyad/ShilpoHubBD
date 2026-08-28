using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.DTOs.ArVr;

public class MediaInput
{
    public string MediaUrl { get; set; } = string.Empty;
    public ArVrMediaType MediaType { get; set; }
    public string? Caption { get; set; }
}
