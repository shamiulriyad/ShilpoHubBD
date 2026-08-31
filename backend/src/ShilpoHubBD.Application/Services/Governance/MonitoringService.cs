using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO monitoring: rule-based fraud / fake-product / review-abuse / QR-anomaly scans
/// that raise <see cref="MonitoringFlag"/>s, manual flag creation, the flag triage workflow, and a
/// read-only QR verification overview. Scans dedupe against still-open flags so re-running is safe.
/// </summary>
public class MonitoringService : IMonitoringService
{
    private const int DefaultScanLookbackDays = 180;
    private const decimal DefaultMinRiskScore = 40m;
    private const int QrOverviewTopN = 15;

    private readonly IMonitoringRepository _repository;

    public MonitoringService(IMonitoringRepository repository)
    {
        _repository = repository;
    }

    public async Task<MonitoringScanResultDto> RunScanAsync(
        Guid userId, RunMonitoringScanRequest request, CancellationToken cancellationToken)
    {
        var scanType = (request.ScanType ?? "All").Trim();
        var validTypes = new[] { "All", "Fraud", "FakeProduct", "ReviewAbuse", "QrAnomaly" };
        if (!validTypes.Contains(scanType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ConflictException($"ScanType must be one of: {string.Join(", ", validTypes)}.");
        }

        var since = request.Since.HasValue
            ? DateTime.SpecifyKind(request.Since.Value, DateTimeKind.Utc)
            : DateTime.UtcNow.AddDays(-DefaultScanLookbackDays);
        var minScore = request.MinRiskScore ?? DefaultMinRiskScore;

        var candidates = new List<ScanCandidate>();
        if (Matches(scanType, "Fraud"))
        {
            candidates.AddRange(await _repository.FindFraudCandidatesAsync(since, cancellationToken));
        }

        if (Matches(scanType, "FakeProduct"))
        {
            candidates.AddRange(await _repository.FindFakeProductCandidatesAsync(since, cancellationToken));
        }

        if (Matches(scanType, "ReviewAbuse"))
        {
            candidates.AddRange(await _repository.FindReviewAbuseCandidatesAsync(since, cancellationToken));
        }

        if (Matches(scanType, "QrAnomaly"))
        {
            candidates.AddRange(await _repository.FindQrAnomalyCandidatesAsync(since, cancellationToken));
        }

        var evaluated = candidates.Count;
        var belowThreshold = candidates.RemoveAll(c => c.RiskScore < minScore);

        var existingKeys = await _repository.GetOpenFlagDedupeKeysAsync(
            candidates.Select(c => c.DedupeKey), cancellationToken);

        var now = DateTime.UtcNow;
        var created = new List<MonitoringFlag>();
        var seen = new HashSet<string>();
        var duplicates = 0;

        foreach (var c in candidates)
        {
            if (existingKeys.Contains(c.DedupeKey) || !seen.Add(c.DedupeKey))
            {
                duplicates++;
                continue;
            }

            var flag = new MonitoringFlag
            {
                Id = Guid.NewGuid(),
                FlagType = c.FlagType,
                Severity = c.Severity,
                Status = MonitoringFlagStatus.Open,
                Source = MonitoringFlagSource.AutomatedScan,
                SubjectType = c.SubjectType,
                SubjectId = c.SubjectId,
                SubjectLabel = Trim(c.SubjectLabel, 200),
                Title = Trim(c.Title, 200),
                Description = Trim(c.Description, 2000),
                EvidenceJson = c.EvidenceJson,
                RiskScore = Math.Clamp(c.RiskScore, 0, 100),
                DedupeKey = c.DedupeKey,
                DetectedAt = now,
                CreatedByUserId = userId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            flag.Events.Add(new MonitoringFlagEvent
            {
                Id = Guid.NewGuid(),
                MonitoringFlagId = flag.Id,
                Type = MonitoringFlagEventType.Created,
                Note = "Raised by automated scan.",
                ActorUserId = userId,
                CreatedAt = now,
            });

            await _repository.AddFlagAsync(flag, cancellationToken);
            created.Add(flag);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return new MonitoringScanResultDto
        {
            ScanType = scanType,
            RanAt = now,
            Since = since,
            CandidatesEvaluated = evaluated,
            FlagsCreated = created.Count,
            DuplicatesSkipped = duplicates,
            BelowThresholdSkipped = belowThreshold,
            CreatedFlags = created.Select(f => f.ToListItemDto()).ToList(),
        };
    }

    public async Task<MonitoringFlagDto> CreateFlagAsync(
        Guid userId, CreateMonitoringFlagRequest request, CancellationToken cancellationToken)
    {
        var flagType = ParseEnum<MonitoringFlagType>(request.FlagType, "Invalid FlagType.");
        var severity = ParseEnum<MonitoringFlagSeverity>(request.Severity, "Invalid Severity.");
        var subjectType = ParseEnum<MonitoringSubjectType>(request.SubjectType, "Invalid SubjectType.");

        var now = DateTime.UtcNow;
        var flag = new MonitoringFlag
        {
            Id = Guid.NewGuid(),
            FlagType = flagType,
            Severity = severity,
            Status = MonitoringFlagStatus.Open,
            Source = MonitoringFlagSource.ManualReport,
            SubjectType = subjectType,
            SubjectId = request.SubjectId,
            SubjectLabel = Trim(request.SubjectLabel, 200),
            Title = Trim(request.Title, 200),
            Description = Trim(request.Description, 2000),
            RiskScore = Math.Clamp(request.RiskScore ?? 50m, 0, 100),
            DedupeKey = $"manual:{Guid.NewGuid():N}",
            DetectedAt = now,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        flag.Events.Add(new MonitoringFlagEvent
        {
            Id = Guid.NewGuid(),
            MonitoringFlagId = flag.Id,
            Type = MonitoringFlagEventType.Created,
            Note = "Raised manually.",
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.AddFlagAsync(flag, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetFlagByIdAsync(flag.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<MonitoringFlagListItemDto>> GetFlagsAsync(
        MonitoringFlagQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetFlagsPagedAsync(query, cancellationToken);

        return new PagedResult<MonitoringFlagListItemDto>
        {
            Items = items.Select(f => f.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<MonitoringFlagDto> GetFlagByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadAsync(id, cancellationToken)).ToDto();

    public async Task<MonitoringFlagDto> UpdateFlagStatusAsync(
        Guid userId, Guid id, UpdateMonitoringFlagStatusRequest request, CancellationToken cancellationToken)
    {
        var flag = await LoadAsync(id, cancellationToken);
        var newStatus = ParseEnum<MonitoringFlagStatus>(request.Status, "Invalid Status.");

        if (newStatus == flag.Status)
        {
            throw new ConflictException($"Flag is already {newStatus}.");
        }

        var now = DateTime.UtcNow;
        var previous = flag.Status;
        flag.Status = newStatus;
        flag.UpdatedAt = now;

        if (newStatus is MonitoringFlagStatus.Resolved or MonitoringFlagStatus.Dismissed)
        {
            flag.ResolvedAt = now;
            flag.ResolvedByUserId = userId;
            flag.ResolutionNotes = request.Note?.Trim();
        }
        else
        {
            flag.ResolvedAt = null;
            flag.ResolvedByUserId = null;
        }

        flag.Events.Add(new MonitoringFlagEvent
        {
            Id = Guid.NewGuid(),
            MonitoringFlagId = flag.Id,
            Type = newStatus is MonitoringFlagStatus.Resolved or MonitoringFlagStatus.Dismissed
                ? MonitoringFlagEventType.Resolved
                : MonitoringFlagEventType.StatusChanged,
            Note = request.Note?.Trim(),
            FromStatus = previous,
            ToStatus = newStatus,
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetFlagByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<MonitoringFlagDto> AssignFlagAsync(
        Guid userId, Guid id, AssignMonitoringFlagRequest request, CancellationToken cancellationToken)
    {
        var flag = await LoadAsync(id, cancellationToken);

        if (!await _repository.UserExistsAsync(request.AssigneeUserId, cancellationToken))
        {
            throw new NotFoundException("Assignee user not found.");
        }

        var now = DateTime.UtcNow;
        flag.AssignedToUserId = request.AssigneeUserId;
        if (flag.Status == MonitoringFlagStatus.Open)
        {
            flag.Status = MonitoringFlagStatus.UnderReview;
        }

        flag.UpdatedAt = now;
        flag.Events.Add(new MonitoringFlagEvent
        {
            Id = Guid.NewGuid(),
            MonitoringFlagId = flag.Id,
            Type = MonitoringFlagEventType.Assigned,
            Note = request.Note?.Trim(),
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetFlagByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<MonitoringFlagDto> AddFlagNoteAsync(
        Guid userId, Guid id, AddMonitoringFlagNoteRequest request, CancellationToken cancellationToken)
    {
        var flag = await LoadAsync(id, cancellationToken);
        var now = DateTime.UtcNow;

        flag.UpdatedAt = now;
        flag.Events.Add(new MonitoringFlagEvent
        {
            Id = Guid.NewGuid(),
            MonitoringFlagId = flag.Id,
            Type = MonitoringFlagEventType.CommentAdded,
            Note = request.Note.Trim(),
            ActorUserId = userId,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetFlagByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task DeleteFlagAsync(Guid id, CancellationToken cancellationToken)
    {
        var flag = await LoadAsync(id, cancellationToken);
        _repository.RemoveFlag(flag);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task<QrMonitoringOverviewDto> GetQrOverviewAsync(
        DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        if (from.HasValue)
        {
            from = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
        }

        if (to.HasValue)
        {
            to = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
        }

        if (from.HasValue && to.HasValue && to <= from)
        {
            throw new ConflictException("'to' must be after 'from'.");
        }

        return _repository.GetQrOverviewAsync(from, to, QrOverviewTopN, cancellationToken);
    }

    // ---- helpers -------------------------------------------------------

    private async Task<MonitoringFlag> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetFlagByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Monitoring flag not found.");

    private static bool Matches(string scanType, string target)
        => scanType.Equals("All", StringComparison.OrdinalIgnoreCase)
            || scanType.Equals(target, StringComparison.OrdinalIgnoreCase);

    private static string Trim(string value, int max)
    {
        value = (value ?? string.Empty).Trim();
        return value.Length > max ? value[..max] : value;
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
