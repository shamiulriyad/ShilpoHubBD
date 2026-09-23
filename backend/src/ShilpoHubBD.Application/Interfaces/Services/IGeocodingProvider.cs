using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Resolves a free-text place name to real coordinates. Returns null on no match, a network
// failure, or a timeout -- callers must treat null as "location not found", never fall back to a
// guessed or fabricated point.
public interface IGeocodingProvider
{
    Task<GeoPointDto?> GeocodeAsync(string query, CancellationToken cancellationToken);
}
