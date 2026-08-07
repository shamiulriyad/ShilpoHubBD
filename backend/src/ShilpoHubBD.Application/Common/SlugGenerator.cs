using System.Text;
using System.Text.RegularExpressions;

namespace ShilpoHubBD.Application.Common;

public static class SlugGenerator
{
    public static string Generate(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder();
        var lastWasHyphen = false;

        foreach (var c in normalized)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                lastWasHyphen = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        slug = Regex.Replace(slug, "-{2,}", "-");

        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }
}
