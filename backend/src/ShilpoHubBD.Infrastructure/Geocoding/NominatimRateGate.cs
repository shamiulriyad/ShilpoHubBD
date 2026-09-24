namespace ShilpoHubBD.Infrastructure.Geocoding;

// Nominatim's usage policy asks for no more than ~1 request/second, process-wide -- not per
// request. This singleton serialises outbound calls across every concurrent user of our API so we
// stay a good citizen of the free public instance regardless of our own traffic.
public class NominatimRateGate
{
    private static readonly TimeSpan MinimumGap = TimeSpan.FromMilliseconds(1100);
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTime _lastCallUtc = DateTime.MinValue;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var elapsed = DateTime.UtcNow - _lastCallUtc;
            if (elapsed < MinimumGap)
            {
                await Task.Delay(MinimumGap - elapsed, cancellationToken);
            }

            _lastCallUtc = DateTime.UtcNow;
        }
        finally
        {
            _gate.Release();
        }
    }
}
