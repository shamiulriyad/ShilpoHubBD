namespace ShilpoHubBD.Domain.Entities.Cms;

/// <summary>
/// One admin-managed piece of public-site copy (About page block, footer link, travel resource...).
/// Items are grouped by <see cref="Group"/>; the frontend decides how each group is laid out.
/// </summary>
public class SiteContentItem
{
    public Guid Id { get; set; }

    /// <summary>One of <see cref="SiteContentGroups"/>.</summary>
    public string Group { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Body { get; set; }
    public string? LinkUrl { get; set; }

    /// <summary>Group-specific extra value (e.g. the back of a flip card, or "highlight").</summary>
    public string? Extra { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class SiteContentGroups
{
    public const string AboutStat = "about-stat";
    public const string AboutPurpose = "about-purpose";
    public const string AboutLandscape = "about-landscape";
    public const string AboutStakeholder = "about-stakeholder";
    public const string AboutCapability = "about-capability";
    public const string FooterAbout = "footer-about";
    public const string FooterExplore = "footer-explore";
    public const string FooterMarketplace = "footer-marketplace";
    public const string FooterResources = "footer-resources";
    public const string TravelResource = "travel-resource";

    public static readonly string[] All =
    {
        AboutStat, AboutPurpose, AboutLandscape, AboutStakeholder, AboutCapability,
        FooterAbout, FooterExplore, FooterMarketplace, FooterResources, TravelResource,
    };
}
