using System.Text.RegularExpressions;
using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Application.Services.PerformingArts;

public record PerformingArtMatch(PerformingArtRecord Record, int Score, bool NamedDirectly);

/// <summary>Keyword retrieval over Published performing-arts records for the AI Heritage Assistant. English and
/// Bangla names, category vocabulary, communities, instruments and places are scored; nothing is generated, so an
/// answer can only repeat what a reviewed record says.</summary>
public static class PerformingArtMatcher
{
    /// <summary>Minimum score for the assistant to answer from performing-arts records instead of the RAG service.</summary>
    public const int AnswerThreshold = 40;

    private const int NameScore = 100;
    private const int CategoryScore = 40;
    private const int GenericScore = 30;

    private static readonly Dictionary<PerformingArtCategory, string[]> CategoryKeywords = new()
    {
        [PerformingArtCategory.FolkMusic] = new[]
        {
            "folk music", "folk song", "music", "musical", "song", "singer", "baul", "bhawaiya", "bhatiali",
            "লোকসংগীত", "লোকগীতি", "সংগীত", "গান", "বাউল", "ভাওয়াইয়া", "ভাটিয়ালি",
        },
        [PerformingArtCategory.FolkTheatre] = new[]
        {
            "folk theatre", "folk theater", "theatre", "theater", "drama", "jatra", "pala",
            "লোকনাট্য", "নাট্য", "নাটক", "যাত্রা", "পালা",
        },
        [PerformingArtCategory.TraditionalDance] = new[] { "dance", "dancing", "dancer", "নৃত্য", "নাচ" },
        [PerformingArtCategory.OralPerformance] = new[]
        {
            "oral tradition", "oral performance", "storytelling", "story telling", "recitation", "puthi", "kobigan", "kavigan",
            "মৌখিক", "কবিগান", "পুঁথি", "কথকতা",
        },
        [PerformingArtCategory.RegionalTradition] = new[] { "ritual performance", "regional performance", "আঞ্চলিক পরিবেশনা" },
    };

    private static readonly string[] GenericKeywords =
    {
        "performing art", "performance tradition", "folk performance", "traditional performance", "পরিবেশনা",
    };

    public static readonly IReadOnlyDictionary<string, string> DivisionBangla = new Dictionary<string, string>
    {
        ["Barishal"] = "বরিশাল", ["Chattogram"] = "চট্টগ্রাম", ["Dhaka"] = "ঢাকা", ["Khulna"] = "খুলনা",
        ["Mymensingh"] = "ময়মনসিংহ", ["Rajshahi"] = "রাজশাহী", ["Rangpur"] = "রংপুর", ["Sylhet"] = "সিলেট",
    };

    public static List<PerformingArtMatch> Rank(string question, IEnumerable<PerformingArtRecord> records, int max = 5)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return new List<PerformingArtMatch>();
        }

        var q = Regex.Replace(question, @"\s+", " ").Trim();
        var mentionedCategories = CategoryKeywords.Where(kv => kv.Value.Any(k => Mentions(q, k))).Select(kv => kv.Key).ToHashSet();
        var generic = GenericKeywords.Any(k => Mentions(q, k));

        return records
            .Where(r => r.Status == PerformingArtStatus.Published)
            .Select(r => Score(r, q, mentionedCategories, generic))
            .Where(m => m.Score >= AnswerThreshold)
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Record.DisplayOrder)
            .ThenBy(m => m.Record.TitleEn)
            .Take(max)
            .ToList();
    }

    private static PerformingArtMatch Score(PerformingArtRecord r, string q, HashSet<PerformingArtCategory> categories, bool generic)
    {
        var names = new[] { r.TitleEn, r.TitleBn }.Concat(Split(r.AlternateNames));
        var named = names.Any(n => !string.IsNullOrWhiteSpace(n) && n.Trim().Length >= 3 && Mentions(q, n.Trim()));

        var score = named ? NameScore : 0;
        if (categories.Contains(r.Category)) score += CategoryScore;
        if (generic) score += GenericScore;
        score += Math.Min(50, Split(r.Communities).Count(c => c.Length >= 3 && Mentions(q, c)) * 25);
        score += Math.Min(40, Split(r.Instruments).Count(i => i.Length >= 3 && Mentions(q, i)) * 20);
        if (r.District != null && Mentions(q, r.District.Name)) score += 15;
        if (r.Division != null && (Mentions(q, r.Division) || (DivisionBangla.TryGetValue(r.Division, out var bn) && Mentions(q, bn)))) score += 10;
        if (!string.IsNullOrWhiteSpace(r.Upazila) && Mentions(q, r.Upazila)) score += 10;

        return new PerformingArtMatch(r, score, named);
    }

    private static IEnumerable<string> Split(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Enumerable.Empty<string>()
            : value.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Whole-word match. Latin terms also accept a plural/possessive "s"; Bangla terms accept any trailing
    /// inflection (e.g. যাত্রার, গানের), since Bangla case endings attach directly to the word.</summary>
    internal static bool Mentions(string text, string term)
    {
        var escaped = Regex.Escape(term.Trim());
        var tail = PerformingArtRules.ContainsBangla(term) ? string.Empty : @"(?:'?s)?(?![\p{L}\p{M}])";
        return Regex.IsMatch(text, $@"(?<![\p{{L}}\p{{M}}]){escaped}{tail}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

/// <summary>Builds a source-grounded assistant answer from matched records — every sentence is a stored field.</summary>
public static class PerformingArtAnswerComposer
{
    public const string AnswerCategory = "performing_arts";
    public const string DetailPathPrefix = "/explore/performing-arts/";

    private static readonly Dictionary<PerformingArtCategory, (string En, string Bn)> CategoryLabels = new()
    {
        [PerformingArtCategory.FolkMusic] = ("folk music", "লোকসংগীত"),
        [PerformingArtCategory.FolkTheatre] = ("folk theatre", "লোকনাট্য"),
        [PerformingArtCategory.TraditionalDance] = ("traditional dance", "ঐতিহ্যবাহী নৃত্য"),
        [PerformingArtCategory.OralPerformance] = ("oral performance", "মৌখিক পরিবেশনা"),
        [PerformingArtCategory.RegionalTradition] = ("regional performance tradition", "আঞ্চলিক পরিবেশনা ঐতিহ্য"),
    };

    public static HeritageAssistantAnswerDto Compose(string question, IReadOnlyList<PerformingArtMatch> matches)
    {
        var bangla = PerformingArtRules.ContainsBangla(question);
        var named = matches.Where(m => m.NamedDirectly).ToList();
        var used = named.Count > 0 ? named.Take(2).Select(m => m.Record).ToList() : matches.Select(m => m.Record).ToList();

        string answer;
        if (named.Count > 0)
        {
            answer = string.Join("\n\n", used.Select(r => Describe(r, bangla)));
        }
        else
        {
            var intro = bangla
                ? "আপনার প্রশ্নের সাথে মিলে যাওয়া যাচাইকৃত পরিবেশনা-শিল্প রেকর্ড:"
                : "These reviewed performing-arts records match your question:";
            var lines = used.Select(r => $"• {Title(r, bangla)} ({Place(r, bangla) ?? Label(r.Category, bangla)}): {FirstSentence(Summary(r, bangla))}");
            answer = intro + "\n" + string.Join("\n", lines);
            if (bangla && used.Any(r => r.SummaryBn == null))
            {
                answer += "\n" + MissingBanglaNote;
            }
        }

        return new HeritageAssistantAnswerDto
        {
            Answer = answer,
            Category = AnswerCategory,
            Sources = used.SelectMany(r => r.Sources.OrderBy(s => s.DisplayOrder))
                .Select(s => string.IsNullOrWhiteSpace(s.Publisher) || s.Title.Contains(s.Publisher, StringComparison.OrdinalIgnoreCase)
                    ? s.Title : $"{s.Title} — {s.Publisher}")
                .Distinct()
                .Take(6)
                .ToList(),
            Links = matches.Select(m => m.Record)
                .Select(r => new HeritageAssistantLinkDto { Title = Title(r, bangla), Url = DetailPathPrefix + r.Slug })
                .ToList(),
            RelatedDistricts = used.Where(r => r.District != null).Select(r => r.District!.Name).Distinct().ToList(),
        };
    }

    private const string MissingBanglaNote = "(যাচাইকৃত বাংলা বিবরণ এখনও যোগ করা হয়নি, তাই ইংরেজি বিবরণ দেখানো হলো।)";

    private static string Describe(PerformingArtRecord r, bool bangla)
    {
        var parts = new List<string>();
        var place = Place(r, bangla);
        parts.Add(bangla
            ? $"{Title(r, true)}: {Label(r.Category, true)}" + (place != null ? $", {place}।" : "।")
            : $"{Title(r, false)} is recorded as {Article(Label(r.Category, false))}" + (place != null ? $" from {place}." : "."));

        if (bangla && r.SummaryBn == null)
        {
            parts.Add(MissingBanglaNote);
        }

        parts.Add(Summary(r, bangla));
        if (!string.IsNullOrWhiteSpace(r.Communities)) parts.Add((bangla ? "সংশ্লিষ্ট সম্প্রদায়: " : "Associated communities: ") + r.Communities + ".");
        if (!string.IsNullOrWhiteSpace(r.Instruments)) parts.Add((bangla ? "বাদ্যযন্ত্র ও উপকরণ: " : "Instruments and associated elements: ") + r.Instruments + ".");
        if (!string.IsNullOrWhiteSpace(r.ViabilityNote)) parts.Add((bangla ? "বর্তমান অবস্থা (উৎস অনুযায়ী): " : "Present condition (as recorded): ") + r.ViabilityNote.TrimEnd('.') + ".");
        return string.Join(" ", parts);
    }

    private static string Title(PerformingArtRecord r, bool bangla)
        => r.TitleBn == null ? r.TitleEn : bangla ? $"{r.TitleBn} ({r.TitleEn})" : $"{r.TitleEn} ({r.TitleBn})";

    private static string Summary(PerformingArtRecord r, bool bangla) => bangla && r.SummaryBn != null ? r.SummaryBn : r.SummaryEn;

    private static string Label(PerformingArtCategory category, bool bangla)
        => bangla ? CategoryLabels[category].Bn : CategoryLabels[category].En;

    private static string Article(string label) => ("aeiou".Contains(label[0]) ? "an " : "a ") + label;

    private static string? Place(PerformingArtRecord r, bool bangla)
    {
        var division = r.Division == null ? null
            : bangla ? (PerformingArtMatcher.DivisionBangla.TryGetValue(r.Division, out var bn) ? $"{bn} বিভাগ" : r.Division)
            : $"{r.Division} Division";
        var district = r.District?.Name;
        return district != null && division != null ? $"{district}, {division}" : district ?? division;
    }

    private static string FirstSentence(string text)
    {
        var match = Regex.Match(text, @"^.+?[.।!?](?=\s|$)");
        return match.Success ? match.Value : text;
    }
}
