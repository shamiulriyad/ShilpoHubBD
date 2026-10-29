using System.Linq.Expressions;
using ShilpoHubBD.Application.DTOs.PerformingArts;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Application.Services.PerformingArts;

/// <summary>Validation rules and query filters shared by the validators, the service and the AI retrieval.</summary>
public static class PerformingArtRules
{
    /// <summary>The eight administrative divisions, spelled as in the Districts table.</summary>
    public static readonly IReadOnlyList<string> Divisions =
        new[] { "Barishal", "Chattogram", "Dhaka", "Khulna", "Mymensingh", "Rajshahi", "Rangpur", "Sylhet" };

    /// <summary>Slugs that would collide with sibling routes under api/performing-arts.</summary>
    public static readonly IReadOnlyList<string> ReservedSlugs = new[] { "featured", "facets", "admin", "new" };

    public const int MaxSources = 20;
    public const int MaxMedia = 20;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AudioExtensions = { ".mp3", ".ogg", ".oga", ".wav", ".m4a", ".aac" };
    private static readonly string[] VideoExtensions = { ".mp4", ".webm" };
    private static readonly string[] VideoHosts = { "youtube.com", "www.youtube.com", "m.youtube.com", "youtu.be", "vimeo.com", "player.vimeo.com" };

    public static bool ContainsBangla(string? value)
        => !string.IsNullOrEmpty(value) && value.Any(c => c >= 'ঀ' && c <= '৿');

    /// <summary>An absolute http(s) URL — the only scheme a citation or credit link may use.</summary>
    public static bool IsWebUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    /// <summary>Media must be an https URL or a platform upload path, and its type must match the file/host.</summary>
    public static bool IsValidMediaUrl(PerformingArtMediaType type, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string path;
        string? host = null;
        if (value.StartsWith("/uploads/", StringComparison.Ordinal))
        {
            if (value.Contains("..", StringComparison.Ordinal) || value.Contains('\\'))
            {
                return false;
            }

            path = value;
        }
        else if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
        {
            path = uri.AbsolutePath;
            host = uri.Host.ToLowerInvariant();
        }
        else
        {
            return false;
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        return type switch
        {
            PerformingArtMediaType.Image => ImageExtensions.Contains(extension),
            PerformingArtMediaType.Audio => AudioExtensions.Contains(extension),
            PerformingArtMediaType.Video => VideoExtensions.Contains(extension) || (host != null && VideoHosts.Contains(host)),
            _ => false,
        };
    }

    public static string NormaliseSlug(string slug) => slug.Trim().ToLowerInvariant();

    /// <summary>One filter used for every listing. Translates to SQL (LOWER(..) LIKE) and runs unchanged over
    /// in-memory collections, so the regression tests exercise the real search logic.</summary>
    public static Expression<Func<PerformingArtRecord, bool>> BuildFilter(PerformingArtQueryParameters query, bool publishedOnly)
    {
        var term = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim().ToLower();
        var division = string.IsNullOrWhiteSpace(query.Division) ? null : query.Division.Trim();
        var category = query.Category;
        var districtId = query.DistrictId;
        var featured = query.Featured;
        var status = publishedOnly ? PerformingArtStatus.Published : query.Status;

        return r =>
            (status == null || r.Status == status)
            && (category == null || r.Category == category)
            && (division == null || r.Division == division)
            && (districtId == null || r.DistrictId == districtId)
            && (featured == null || r.IsFeatured == featured)
            && (term == null
                || r.TitleEn.ToLower().Contains(term)
                || (r.TitleBn != null && r.TitleBn.Contains(term))
                || (r.AlternateNames != null && r.AlternateNames.ToLower().Contains(term))
                || r.SummaryEn.ToLower().Contains(term)
                || (r.SummaryBn != null && r.SummaryBn.Contains(term))
                || (r.Communities != null && r.Communities.ToLower().Contains(term))
                || (r.Artists != null && r.Artists.ToLower().Contains(term))
                || (r.Instruments != null && r.Instruments.ToLower().Contains(term))
                || (r.Upazila != null && r.Upazila.ToLower().Contains(term))
                || (r.Division != null && r.Division.ToLower().Contains(term))
                || (r.District != null && r.District.Name.ToLower().Contains(term)));
    }
}
