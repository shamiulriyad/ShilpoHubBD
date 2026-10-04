namespace ShilpoHubBD.Application.Common;

public sealed record ExplorerTier(string Name, int MinimumSites, int? NextTierAt, decimal DiscountPercent);

public static class ExplorerTierPolicy
{
    public static ExplorerTier Resolve(int uniqueSitesVisited) => uniqueSitesVisited switch
    {
        >= 10 => new("Diamond", 10, null, 15m),
        >= 5 => new("Gold", 5, 10, 10m),
        >= 1 => new("Silver", 1, 5, 5m),
        _ => new("Explorer", 0, 1, 0m),
    };
}
