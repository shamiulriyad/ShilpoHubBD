using System.Globalization;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Infrastructure.ResearchAI;

/// <summary>
/// Rule-based stand-in for a future AI/ML backend behind the AI Research Assistant. Every method
/// derives its answer from simple statistics and heuristics over the context it is handed -- no
/// external calls, no model weights. Swap for a real <see cref="IResearchAIProvider"/> later without
/// touching ResearchAIService or the controller.
/// </summary>
public class DummyResearchAIProvider : IResearchAIProvider
{
    public const string Name = "dummy-rule-based-v1";

    public Task<ResearchAnalysisResult> GenerateInsightsAsync(
        ResearchAnalysisContext context, CancellationToken cancellationToken)
    {
        var items = new List<ResearchFindingItem>();
        var series = ExtractSeries(context.SelectedData);
        var allValues = series.SelectMany(s => s.Values).ToList();

        if (allValues.Count > 0)
        {
            var mean = allValues.Average();
            var min = allValues.Min();
            var max = allValues.Max();
            var stdev = StandardDeviation(allValues, mean);
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Insight,
                Heading = "Descriptive summary of the selected data",
                Detail = $"Across {allValues.Count} numeric point(s) in {series.Count} series, values range "
                    + $"from {Fmt(min)} to {Fmt(max)} with a mean of {Fmt(mean)} "
                    + $"(standard deviation {Fmt(stdev)}).",
                Metric = $"n = {allValues.Count}",
                Score = 0.9,
            });

            var spread = max - min;
            if (spread > 0 && stdev / (Math.Abs(mean) < 1e-9 ? 1 : Math.Abs(mean)) > 0.5)
            {
                items.Add(new ResearchFindingItem
                {
                    Category = ResearchAIFindingCategory.Insight,
                    Heading = "High variability detected",
                    Detail = "The coefficient of variation exceeds 0.5, so the selected data is quite dispersed. "
                        + "Consider segmenting by district, craft or time period before drawing conclusions.",
                    Metric = $"CV ~= {Fmt(stdev / (Math.Abs(mean) < 1e-9 ? 1 : Math.Abs(mean)))}",
                    Score = 0.6,
                });
            }

            var topCategory = context.SelectedData
                .Where(d => !string.IsNullOrWhiteSpace(d.Category) && d.NumericValue.HasValue)
                .GroupBy(d => d.Category!)
                .Select(g => new { Category = g.Key, Total = g.Sum(x => x.NumericValue!.Value) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();
            if (topCategory is not null)
            {
                items.Add(new ResearchFindingItem
                {
                    Category = ResearchAIFindingCategory.Insight,
                    Heading = $"\"{topCategory.Category}\" dominates the selected data",
                    Detail = $"\"{topCategory.Category}\" accounts for the largest share of the measured values "
                        + $"({Fmt(topCategory.Total)}). It is the natural focus for a first deep-dive.",
                    Score = 0.7,
                });
            }
        }
        else
        {
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Caveat,
                Heading = "No numeric data supplied",
                Detail = "Insights were generated from project and dataset metadata only. Attach selected data "
                    + "points with numeric values for quantitative findings.",
                Score = 0.3,
            });
        }

        if (context.DatasetRecordCount is > 0)
        {
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Insight,
                Heading = "Dataset scale",
                Detail = $"The linked dataset \"{context.DatasetName}\" ({context.DatasetCategory}) currently holds "
                    + $"{context.DatasetRecordCount} record(s). A sample of this size supports district-level "
                    + "aggregation but may be thin for rare crafts.",
                Metric = $"records = {context.DatasetRecordCount}",
                Score = 0.65,
            });
        }

        foreach (var question in context.ResearchQuestions.Take(5))
        {
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Recommendation,
                Heading = "Toward the research question",
                Detail = $"For \"{question.Trim()}\": cross-tabulate the selected data against district and "
                    + "verification status, then compare pre/post a chosen reference date.",
                Score = 0.5,
            });
        }

        AddStandardCaveats(items);

        var summary = allValues.Count > 0
            ? $"Generated {items.Count} rule-based insight(s) from {allValues.Count} data point(s) and project context."
            : $"Generated {items.Count} rule-based insight(s) from project and dataset metadata (no numeric data supplied).";

        return Task.FromResult(new ResearchAnalysisResult
        {
            ProviderName = Name,
            Summary = summary,
            Confidence = allValues.Count > 0 ? 0.55 : 0.3,
            Items = items,
        });
    }

    public Task<ResearchAnalysisResult> DiscoverTrendsAsync(
        ResearchAnalysisContext context, CancellationToken cancellationToken)
    {
        var items = new List<ResearchFindingItem>();
        var series = ExtractSeries(context.SelectedData, requireOrder: true);

        foreach (var s in series.Where(s => s.Values.Count >= 3))
        {
            var first = s.Values.First();
            var last = s.Values.Last();
            var pct = Math.Abs(first) < 1e-9 ? (double?)null : (last - first) / Math.Abs(first) * 100.0;
            var slope = LinearSlope(s.Values);
            var direction = slope > 1e-6 ? "increasing" : slope < -1e-6 ? "decreasing" : "flat";

            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Trend,
                Heading = $"Series \"{s.Name}\" is {direction}",
                Detail = $"Over {s.Values.Count} ordered point(s) the least-squares slope is {Fmt(slope)} per step"
                    + (pct.HasValue ? $", a {Fmt(pct.Value)}% change from first to last observation." : "."),
                Metric = pct.HasValue ? $"{(pct >= 0 ? "+" : "")}{Fmt(pct.Value)}%" : $"slope {Fmt(slope)}",
                Score = Math.Min(1.0, Math.Abs(slope) / (s.Values.Max() - s.Values.Min() + 1e-9) + 0.3),
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Caveat,
                Heading = "Insufficient ordered data for trend discovery",
                Detail = "Provide at least 3 numeric points per series (ideally with timestamps) to estimate a trend. "
                    + $"Falling back to a generic pattern for the \"{context.DatasetCategory ?? "heritage"}\" domain: "
                    + "activity typically peaks around Pohela Boishakh (April) and the Sept-Dec festival/wedding season.",
                Score = 0.25,
            });
        }

        if (context.RangeStart.HasValue && context.RangeEnd.HasValue)
        {
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Caveat,
                Heading = "Window under review",
                Detail = $"Trends were evaluated for {context.RangeStart:yyyy-MM-dd} to {context.RangeEnd:yyyy-MM-dd}. "
                    + "Seasonality shorter than this window can be masked.",
                Score = 0.4,
            });
        }

        return Task.FromResult(new ResearchAnalysisResult
        {
            ProviderName = Name,
            Summary = $"Evaluated {series.Count} series for directional trends; produced {items.Count} finding(s).",
            Confidence = series.Any(s => s.Values.Count >= 3) ? 0.5 : 0.25,
            Items = items,
        });
    }

    public Task<ResearchAnalysisResult> DetectCorrelationsAsync(
        ResearchAnalysisContext context, CancellationToken cancellationToken)
    {
        var items = new List<ResearchFindingItem>();
        var series = ExtractSeries(context.SelectedData, requireOrder: true)
            .Where(s => s.Values.Count >= 3)
            .ToList();

        for (var i = 0; i < series.Count; i++)
        {
            for (var j = i + 1; j < series.Count; j++)
            {
                var n = Math.Min(series[i].Values.Count, series[j].Values.Count);
                var a = series[i].Values.Take(n).ToArray();
                var b = series[j].Values.Take(n).ToArray();
                var r = Pearson(a, b);
                if (double.IsNaN(r))
                {
                    continue;
                }

                items.Add(new ResearchFindingItem
                {
                    Category = ResearchAIFindingCategory.Correlation,
                    Heading = $"\"{series[i].Name}\" vs \"{series[j].Name}\": {StrengthLabel(r)} {(r >= 0 ? "positive" : "negative")} correlation",
                    Detail = $"Pearson r = {Fmt(r)} over {n} aligned point(s). "
                        + "This is an association only; it does not establish causation and is sensitive to outliers at this sample size.",
                    Metric = $"r = {Fmt(r)}",
                    Score = Math.Abs(r),
                });
            }
        }

        if (items.Count == 0)
        {
            items.Add(new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.Caveat,
                Heading = "Not enough parallel series for correlation",
                Detail = "Supply at least two numeric series (distinct \"series\" labels) with 3+ aligned points each.",
                Score = 0.2,
            });
        }
        else
        {
            items = items.OrderByDescending(x => x.Score ?? 0).ToList();
        }

        return Task.FromResult(new ResearchAnalysisResult
        {
            ProviderName = Name,
            Summary = $"Computed {items.Count(x => x.Category == ResearchAIFindingCategory.Correlation)} pairwise correlation(s).",
            Confidence = items.Any(x => x.Category == ResearchAIFindingCategory.Correlation) ? 0.5 : 0.2,
            Items = items,
        });
    }

    public Task<ResearchAnalysisResult> GenerateReportAsync(
        ResearchAnalysisContext context, CancellationToken cancellationToken)
    {
        var series = ExtractSeries(context.SelectedData);
        var allValues = series.SelectMany(s => s.Values).ToList();

        var sections = new List<ResearchFindingItem>
        {
            new()
            {
                Category = ResearchAIFindingCategory.ReportSection,
                Heading = "1. Background",
                Detail = $"Project \"{context.ProjectTitle}\""
                    + (string.IsNullOrWhiteSpace(context.Discipline) ? "" : $" ({context.Discipline})")
                    + ". Research questions under study: "
                    + (context.ResearchQuestions.Count > 0
                        ? string.Join("; ", context.ResearchQuestions.Select(q => q.Trim()))
                        : "not specified") + ".",
            },
            new()
            {
                Category = ResearchAIFindingCategory.ReportSection,
                Heading = "2. Data overview",
                Detail = context.DatasetName is null
                    ? $"{context.SelectedData.Count} selected data point(s) supplied directly; no catalog dataset linked."
                    : $"Linked dataset \"{context.DatasetName}\" ({context.DatasetCategory}, ~{context.DatasetRecordCount} records)"
                        + $"; plus {context.SelectedData.Count} selected data point(s).",
            },
            new()
            {
                Category = ResearchAIFindingCategory.ReportSection,
                Heading = "3. Key findings",
                Detail = allValues.Count > 0
                    ? $"Selected values average {Fmt(allValues.Average())} (min {Fmt(allValues.Min())}, max {Fmt(allValues.Max())}). "
                        + "See the insights and correlation analyses for detail."
                    : "No numeric data was supplied, so findings are qualitative and rest on the project context.",
            },
            new()
            {
                Category = ResearchAIFindingCategory.ReportSection,
                Heading = "4. Limitations",
                Detail = "Results are produced by a deterministic rule-based stand-in, not a trained model. "
                    + "Sample sizes are small, associations are not causal, and no significance testing was performed.",
            },
            new()
            {
                Category = ResearchAIFindingCategory.ReportSection,
                Heading = "5. Recommended next steps",
                Detail = "Expand the sample, add a comparison group, register hypotheses before analysis, and "
                    + "re-run once a real AI provider is connected.",
            },
        };

        if (!string.IsNullOrWhiteSpace(context.PaperTitle))
        {
            sections.Insert(4, new ResearchFindingItem
            {
                Category = ResearchAIFindingCategory.ReportSection,
                Heading = "4b. Relation to the working paper",
                Detail = $"Working paper \"{context.PaperTitle}\". "
                    + (string.IsNullOrWhiteSpace(context.PaperAbstract) ? "" : $"Abstract on file ({context.PaperAbstract!.Length} chars). ")
                    + "Align the findings section above with the paper's stated contributions.",
            });
        }

        return Task.FromResult(new ResearchAnalysisResult
        {
            ProviderName = Name,
            Summary = $"Assembled a {sections.Count}-section draft report from the supplied context.",
            Confidence = 0.5,
            Items = sections,
        });
    }

    public Task<ResearchCitationResult> GenerateCitationsAsync(
        ResearchCitationContext context, CancellationToken cancellationToken)
    {
        var items = context.Sources.Select(s => new GeneratedCitationItem
        {
            Style = context.Style,
            SourceTitle = s.Title,
            Authors = s.Authors,
            Year = s.Year,
            Container = s.Container,
            Doi = s.Doi,
            Url = s.Url,
            ResearchPublicationId = s.ResearchPublicationId,
            FormattedCitation = Format(context.Style, s),
        }).ToList();

        return Task.FromResult(new ResearchCitationResult
        {
            ProviderName = Name,
            Summary = $"Formatted {items.Count} source(s) in {context.Style} style.",
            Items = items,
        });
    }

    // ---- formatting -----------------------------------------------------

    private static string Format(ResearchCitationStyle style, ResearchCitationSourceDto s)
    {
        var authors = string.IsNullOrWhiteSpace(s.Authors) ? "Anon." : s.Authors!.Trim();
        var year = s.Year?.ToString(CultureInfo.InvariantCulture) ?? "n.d.";
        var title = s.Title.Trim().TrimEnd('.');
        var container = string.IsNullOrWhiteSpace(s.Container) ? null : s.Container!.Trim();
        var locator = !string.IsNullOrWhiteSpace(s.Doi)
            ? $"https://doi.org/{s.Doi!.Trim()}"
            : s.Url?.Trim();

        return style switch
        {
            ResearchCitationStyle.Apa =>
                $"{authors} ({year}). {title}." + (container is null ? "" : $" {container}.") + (locator is null ? "" : $" {locator}"),
            ResearchCitationStyle.Mla =>
                $"{authors}. \"{title}.\"" + (container is null ? "" : $" {container},") + $" {year}." + (locator is null ? "" : $" {locator}."),
            ResearchCitationStyle.Chicago =>
                $"{authors}. \"{title}.\"" + (container is null ? "" : $" {container}") + $" ({year})." + (locator is null ? "" : $" {locator}."),
            ResearchCitationStyle.Ieee =>
                $"{authors}, \"{title},\"" + (container is null ? "" : $" {container},") + $" {year}." + (locator is null ? "" : $" [Online]. Available: {locator}"),
            ResearchCitationStyle.Bibtex => BuildBibtex(authors, year, title, container, s),
            _ => $"{authors} ({year}). {title}.",
        };
    }

    private static string BuildBibtex(string authors, string year, string title, string? container, ResearchCitationSourceDto s)
    {
        var key = new string((authors.Split(',', ' ').FirstOrDefault() ?? "ref")
            .Where(char.IsLetterOrDigit).ToArray());
        key = (string.IsNullOrEmpty(key) ? "ref" : key.ToLowerInvariant()) + year.Replace("n.d.", "nd");
        var fields = new List<string> { $"  author = {{{authors}}}", $"  title = {{{title}}}", $"  year = {{{year}}}" };
        if (container is not null)
        {
            fields.Add($"  journal = {{{container}}}");
        }

        if (!string.IsNullOrWhiteSpace(s.Doi))
        {
            fields.Add($"  doi = {{{s.Doi!.Trim()}}}");
        }

        if (!string.IsNullOrWhiteSpace(s.Url))
        {
            fields.Add($"  url = {{{s.Url!.Trim()}}}");
        }

        return $"@article{{{key},\n{string.Join(",\n", fields)}\n}}";
    }

    // ---- statistics helpers ------------------------------------------

    private sealed record Series(string Name, List<double> Values);

    private static List<Series> ExtractSeries(List<ResearchDataPointDto> data, bool requireOrder = false)
    {
        var groups = data
            .Where(d => d.NumericValue.HasValue)
            .Select((d, index) => (d, index))
            .GroupBy(x => string.IsNullOrWhiteSpace(x.d.Series) ? "default" : x.d.Series!.Trim());

        var result = new List<Series>();
        foreach (var g in groups)
        {
            var ordered = requireOrder
                ? g.OrderBy(x => x.d.Timestamp ?? DateTime.MinValue).ThenBy(x => x.index)
                : g.OrderBy(x => x.index);
            result.Add(new Series(g.Key, ordered.Select(x => x.d.NumericValue!.Value).ToList()));
        }

        return result;
    }

    private static double StandardDeviation(IReadOnlyCollection<double> values, double mean)
    {
        if (values.Count < 2)
        {
            return 0;
        }

        var variance = values.Sum(v => (v - mean) * (v - mean)) / (values.Count - 1);
        return Math.Sqrt(variance);
    }

    private static double LinearSlope(IReadOnlyList<double> values)
    {
        var n = values.Count;
        if (n < 2)
        {
            return 0;
        }

        double sumX = 0, sumY = 0, sumXy = 0, sumXx = 0;
        for (var i = 0; i < n; i++)
        {
            sumX += i;
            sumY += values[i];
            sumXy += i * values[i];
            sumXx += (double)i * i;
        }

        var denom = n * sumXx - sumX * sumX;
        return Math.Abs(denom) < 1e-9 ? 0 : (n * sumXy - sumX * sumY) / denom;
    }

    private static double Pearson(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        var n = Math.Min(a.Count, b.Count);
        if (n < 2)
        {
            return double.NaN;
        }

        double meanA = 0, meanB = 0;
        for (var i = 0; i < n; i++)
        {
            meanA += a[i];
            meanB += b[i];
        }

        meanA /= n;
        meanB /= n;

        double cov = 0, varA = 0, varB = 0;
        for (var i = 0; i < n; i++)
        {
            var da = a[i] - meanA;
            var db = b[i] - meanB;
            cov += da * db;
            varA += da * da;
            varB += db * db;
        }

        var denom = Math.Sqrt(varA * varB);
        return denom < 1e-12 ? double.NaN : cov / denom;
    }

    private static string StrengthLabel(double r)
    {
        var a = Math.Abs(r);
        return a switch
        {
            < 0.2 => "negligible",
            < 0.4 => "weak",
            < 0.6 => "moderate",
            < 0.8 => "strong",
            _ => "very strong",
        };
    }

    private static void AddStandardCaveats(List<ResearchFindingItem> items)
    {
        items.Add(new ResearchFindingItem
        {
            Category = ResearchAIFindingCategory.Caveat,
            Heading = "Method note",
            Detail = "These results come from a deterministic rule-based provider, not a trained model. "
                + "Treat them as a starting checklist, not evidence.",
            Score = 0.2,
        });
    }

    private static string Fmt(double value) => Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);
}
