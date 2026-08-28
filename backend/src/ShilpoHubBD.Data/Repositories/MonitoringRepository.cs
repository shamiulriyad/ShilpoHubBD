using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class MonitoringRepository : IMonitoringRepository
{
    private static readonly OrderStatus[] BadOrderStatuses =
        { OrderStatus.Cancelled, OrderStatus.Returned, OrderStatus.Refunded, OrderStatus.ReturnRequested };

    private readonly ShilpoHubDbContext _context;

    public MonitoringRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Flags ------------------------------------------------------------

    public async Task AddFlagAsync(MonitoringFlag flag, CancellationToken cancellationToken)
        => await _context.MonitoringFlags.AddAsync(flag, cancellationToken);

    public void RemoveFlag(MonitoringFlag flag) => _context.MonitoringFlags.Remove(flag);

    public Task<MonitoringFlag?> GetFlagByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.MonitoringFlags
            .Include(f => f.CreatedBy)
            .Include(f => f.AssignedTo)
            .Include(f => f.ResolvedBy)
            .Include(f => f.Events).ThenInclude(e => e.Actor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<(List<MonitoringFlag> Items, int TotalCount)> GetFlagsPagedAsync(
        MonitoringFlagQueryParameters query, CancellationToken cancellationToken)
    {
        var flags = _context.MonitoringFlags
            .Include(f => f.AssignedTo)
            .AsQueryable();

        if (TryEnum<MonitoringFlagType>(query.FlagType, out var flagType))
        {
            flags = flags.Where(f => f.FlagType == flagType);
        }

        if (TryEnum<MonitoringFlagSeverity>(query.Severity, out var severity))
        {
            flags = flags.Where(f => f.Severity == severity);
        }

        if (TryEnum<MonitoringFlagStatus>(query.Status, out var status))
        {
            flags = flags.Where(f => f.Status == status);
        }

        if (TryEnum<MonitoringSubjectType>(query.SubjectType, out var subjectType))
        {
            flags = flags.Where(f => f.SubjectType == subjectType);
        }

        if (query.SubjectId.HasValue)
        {
            flags = flags.Where(f => f.SubjectId == query.SubjectId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            flags = flags.Where(f => f.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            flags = flags.Where(f => f.Title.ToLower().Contains(term)
                || f.SubjectLabel.ToLower().Contains(term));
        }

        flags = flags
            .OrderByDescending(f => f.Status == MonitoringFlagStatus.Open || f.Status == MonitoringFlagStatus.UnderReview)
            .ThenByDescending(f => f.Severity)
            .ThenByDescending(f => f.DetectedAt);

        var totalCount = await flags.CountAsync(cancellationToken);
        var items = await flags
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<HashSet<string>> GetOpenFlagDedupeKeysAsync(
        IEnumerable<string> candidateKeys, CancellationToken cancellationToken)
    {
        var keys = candidateKeys.Distinct().ToList();
        var existing = await _context.MonitoringFlags
            .Where(f => keys.Contains(f.DedupeKey)
                && f.Status != MonitoringFlagStatus.Dismissed
                && f.Status != MonitoringFlagStatus.Resolved)
            .Select(f => f.DedupeKey)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);

    // ---- Scans ----------------------------------------------------------

    public async Task<List<ScanCandidate>> FindFraudCandidatesAsync(
        DateTime since, CancellationToken cancellationToken)
    {
        var results = new List<ScanCandidate>();

        // Buyers whose recent orders are overwhelmingly cancelled/returned/refunded.
        var buyerRows = await _context.Orders
            .Where(o => o.CreatedAt >= since)
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Total = g.Count(),
                Bad = g.Count(o => BadOrderStatuses.Contains(o.Status)),
                Refunded = g.Sum(o => o.RefundAmount ?? 0m),
            })
            .Where(x => x.Total >= 5 && x.Bad * 100 >= x.Total * 40)
            .ToListAsync(cancellationToken);

        var buyerIds = buyerRows.Select(r => r.UserId).ToList();
        var buyerNames = await _context.Users
            .Where(u => buyerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        foreach (var r in buyerRows)
        {
            var rate = (double)r.Bad / r.Total;
            var name = buyerNames.FirstOrDefault(n => n.Id == r.UserId)?.FullName ?? r.UserId.ToString();
            results.Add(new ScanCandidate(
                MonitoringFlagType.FraudRisk,
                rate >= 0.7 ? MonitoringFlagSeverity.High : MonitoringFlagSeverity.Medium,
                MonitoringSubjectType.Producer, // buyer is still a User; SubjectType Producer bucket = "user account"
                r.UserId,
                name,
                "High order cancellation / refund rate for a buyer account",
                $"{r.Bad} of {r.Total} orders since {since:yyyy-MM-dd} were cancelled, returned or refunded "
                + $"(BDT {r.Refunded:N0} refunded).",
                (decimal)Math.Round(40 + rate * 55, 2),
                JsonSerializer.Serialize(new { r.Total, r.Bad, r.Refunded, rate }),
                $"fraud:buyer:{r.UserId}"));
        }

        // Producers with a high share of cancelled/returned/refunded orders on their items.
        var totalByProducer = await _context.OrderItems
            .Where(i => i.Order.CreatedAt >= since)
            .Select(i => new { i.Product.ProducerId, i.OrderId })
            .Distinct()
            .GroupBy(x => x.ProducerId)
            .Select(g => new { ProducerId = g.Key, Count = g.Count() })
            .Where(x => x.Count >= 8)
            .ToDictionaryAsync(x => x.ProducerId, x => x.Count, cancellationToken);

        var badByProducer = await _context.OrderItems
            .Where(i => i.Order.CreatedAt >= since && BadOrderStatuses.Contains(i.Order.Status))
            .Select(i => new { i.Product.ProducerId, i.OrderId })
            .Distinct()
            .GroupBy(x => x.ProducerId)
            .Select(g => new { ProducerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProducerId, x => x.Count, cancellationToken);

        var producerRows = totalByProducer
            .Select(kv => new
            {
                ProducerId = kv.Key,
                Total = kv.Value,
                Bad = badByProducer.TryGetValue(kv.Key, out var b) ? b : 0,
            })
            .Where(x => x.Bad * 100 >= x.Total * 35)
            .ToList();

        var prodIds = producerRows.Select(r => r.ProducerId).ToList();
        var prodNames = await _context.Users
            .Where(u => prodIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        foreach (var r in producerRows)
        {
            var rate = (double)r.Bad / r.Total;
            var name = prodNames.FirstOrDefault(n => n.Id == r.ProducerId)?.FullName ?? r.ProducerId.ToString();
            results.Add(new ScanCandidate(
                MonitoringFlagType.FraudRisk,
                rate >= 0.6 ? MonitoringFlagSeverity.High : MonitoringFlagSeverity.Medium,
                MonitoringSubjectType.Producer,
                r.ProducerId,
                name,
                "High cancelled / returned order rate on a producer's sales",
                $"{r.Bad} of {r.Total} orders for this producer's items since {since:yyyy-MM-dd} were "
                + "cancelled, returned or refunded.",
                (decimal)Math.Round(35 + rate * 50, 2),
                JsonSerializer.Serialize(new { r.Total, r.Bad, rate }),
                $"fraud:producer-orders:{r.ProducerId}"));
        }

        return results;
    }

    public async Task<List<ScanCandidate>> FindFakeProductCandidatesAsync(
        DateTime since, CancellationToken cancellationToken)
    {
        var results = new List<ScanCandidate>();

        var rejectedActive = await _context.Products
            .Where(p => p.IsActive && p.HandmadeVerificationStatus == HandmadeVerificationStatus.Rejected)
            .Select(p => new { p.Id, p.Name, p.ProducerId, p.SalesCount, ProducerName = p.Producer.FullName })
            .ToListAsync(cancellationToken);

        foreach (var p in rejectedActive)
        {
            var severity = p.SalesCount > 0 ? MonitoringFlagSeverity.High : MonitoringFlagSeverity.Medium;
            results.Add(new ScanCandidate(
                MonitoringFlagType.FakeProduct,
                severity,
                MonitoringSubjectType.Product,
                p.Id,
                p.Name,
                "Active listing failed handmade verification",
                $"Product \"{p.Name}\" by {p.ProducerName} is live despite a Rejected handmade-verification status"
                + (p.SalesCount > 0 ? $" and has {p.SalesCount} recorded sales." : "."),
                p.SalesCount > 0 ? 80 : 60,
                JsonSerializer.Serialize(new { p.SalesCount }),
                $"fake:product:{p.Id}"));
        }

        var repeatOffenders = await _context.Products
            .Where(p => p.HandmadeVerificationStatus == HandmadeVerificationStatus.Rejected)
            .GroupBy(p => new { p.ProducerId, p.Producer.FullName })
            .Select(g => new { g.Key.ProducerId, g.Key.FullName, Count = g.Count() })
            .Where(x => x.Count >= 3)
            .ToListAsync(cancellationToken);

        foreach (var r in repeatOffenders)
        {
            results.Add(new ScanCandidate(
                MonitoringFlagType.FakeProduct,
                r.Count >= 6 ? MonitoringFlagSeverity.High : MonitoringFlagSeverity.Medium,
                MonitoringSubjectType.Producer,
                r.ProducerId,
                r.FullName,
                "Producer with multiple rejected products",
                $"{r.FullName} has {r.Count} products rejected at handmade verification.",
                (decimal)Math.Min(90, 45 + r.Count * 7),
                JsonSerializer.Serialize(new { rejectedProducts = r.Count }),
                $"fake:producer:{r.ProducerId}"));
        }

        return results;
    }

    public async Task<List<ScanCandidate>> FindReviewAbuseCandidatesAsync(
        DateTime since, CancellationToken cancellationToken)
    {
        var results = new List<ScanCandidate>();

        var reviewerRaw = await _context.Reviews
            .Where(r => r.CreatedAt >= since)
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count(),
                RatingSum = g.Sum(r => r.Rating),
                Distinct = g.Select(r => r.ProductId).Distinct().Count(),
            })
            .Where(x => x.Count >= 8)
            .ToListAsync(cancellationToken);

        var reviewerRows = reviewerRaw
            .Select(x => new { x.UserId, x.Count, x.Distinct, AvgRating = (double)x.RatingSum / x.Count })
            .Where(x => x.AvgRating >= 4.5)
            .ToList();

        var ids = reviewerRows.Select(r => r.UserId).ToList();
        var names = await _context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        foreach (var r in reviewerRows)
        {
            var name = names.FirstOrDefault(n => n.Id == r.UserId)?.FullName ?? r.UserId.ToString();
            results.Add(new ScanCandidate(
                MonitoringFlagType.ReviewAbuse,
                MonitoringFlagSeverity.Low,
                MonitoringSubjectType.Review,
                r.UserId,
                name,
                "Possible review rating inflation",
                $"{name} left {r.Count} reviews since {since:yyyy-MM-dd} averaging {r.AvgRating:0.0}/5 "
                + $"across {r.Distinct} products.",
                (decimal)Math.Round(35 + Math.Min(45, r.Count * 2.0), 2),
                JsonSerializer.Serialize(new { r.Count, r.AvgRating, r.Distinct }),
                $"review:user:{r.UserId}"));
        }

        return results;
    }

    public async Task<List<ScanCandidate>> FindQrAnomalyCandidatesAsync(
        DateTime since, CancellationToken cancellationToken)
    {
        var results = new List<ScanCandidate>();

        var productRows = await _context.QRVerificationRecords
            .Where(v => v.VerifiedAt >= since && v.QRCodeId != null)
            .GroupBy(v => v.QRCode!.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Total = g.Count(),
                Invalid = g.Count(v => !v.IsValid),
            })
            .Where(x => x.Total >= 10 && x.Invalid * 100 >= x.Total * 40)
            .ToListAsync(cancellationToken);

        var productIds = productRows.Select(r => r.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cancellationToken);

        foreach (var r in productRows)
        {
            var rate = (double)r.Invalid / r.Total;
            var name = products.FirstOrDefault(p => p.Id == r.ProductId)?.Name ?? r.ProductId.ToString();
            results.Add(new ScanCandidate(
                MonitoringFlagType.QrAnomaly,
                rate >= 0.7 ? MonitoringFlagSeverity.High : MonitoringFlagSeverity.Medium,
                MonitoringSubjectType.Product,
                r.ProductId,
                name,
                "High invalid-QR-scan rate for a product",
                $"{r.Invalid} of {r.Total} QR scans for \"{name}\" since {since:yyyy-MM-dd} were invalid "
                + "— possible counterfeit codes in circulation.",
                (decimal)Math.Round(45 + rate * 50, 2),
                JsonSerializer.Serialize(new { r.Total, r.Invalid, rate }),
                $"qr:product:{r.ProductId}"));
        }

        // Frequently-scanned codes that never resolve to a known QR code.
        var unresolved = await _context.QRVerificationRecords
            .Where(v => v.VerifiedAt >= since && v.QRCodeId == null)
            .GroupBy(v => v.ScannedCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .Where(x => x.Count >= 5)
            .ToListAsync(cancellationToken);

        foreach (var r in unresolved)
        {
            results.Add(new ScanCandidate(
                MonitoringFlagType.QrAnomaly,
                MonitoringFlagSeverity.Medium,
                MonitoringSubjectType.QrCode,
                null,
                r.Code,
                "Unknown QR code scanned repeatedly",
                $"Code \"{r.Code}\" was scanned {r.Count} times since {since:yyyy-MM-dd} but matches no issued QR code.",
                (decimal)Math.Min(85, 40 + r.Count * 4),
                JsonSerializer.Serialize(new { scans = r.Count }),
                $"qr:unknown:{r.Code}"));
        }

        return results;
    }

    // ---- QR overview -------------------------------------------------

    public async Task<QrMonitoringOverviewDto> GetQrOverviewAsync(
        DateTime? from, DateTime? to, int topN, CancellationToken cancellationToken)
    {
        var scans = _context.QRVerificationRecords.AsQueryable();
        if (from.HasValue)
        {
            scans = scans.Where(v => v.VerifiedAt >= from.Value);
        }

        if (to.HasValue)
        {
            scans = scans.Where(v => v.VerifiedAt < to.Value);
        }

        var totalScans = await scans.CountAsync(cancellationToken);
        var validScans = await scans.CountAsync(v => v.IsValid, cancellationToken);
        var unresolved = await scans.CountAsync(v => v.QRCodeId == null, cancellationToken);
        var uniqueScanners = await scans
            .Where(v => v.VerifiedByUserId != null)
            .Select(v => v.VerifiedByUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalCodes = await _context.QRCodes.CountAsync(cancellationToken);
        var activeCodes = await _context.QRCodes.CountAsync(c => c.IsActive, cancellationToken);

        var productStatsRaw = await scans
            .Where(v => v.QRCodeId != null)
            .GroupBy(v => v.QRCode!.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Total = g.Count(),
                Invalid = g.Count(v => !v.IsValid),
            })
            .Where(x => x.Total >= 5 && x.Invalid > 0)
            .ToListAsync(cancellationToken);

        var productStats = productStatsRaw
            .OrderByDescending(x => (double)x.Invalid / x.Total)
            .Take(topN)
            .ToList();

        var statProductIds = productStats.Select(s => s.ProductId).ToList();
        var productMeta = await _context.Products
            .Where(p => statProductIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.ProducerId, ProducerName = p.Producer.FullName })
            .ToListAsync(cancellationToken);

        var anomalousProducts = productStats.Select(s =>
        {
            var meta = productMeta.FirstOrDefault(p => p.Id == s.ProductId);
            return new QrProductStatDto
            {
                ProductId = s.ProductId,
                ProductName = meta?.Name ?? s.ProductId.ToString(),
                ProducerId = meta?.ProducerId ?? Guid.Empty,
                ProducerName = meta?.ProducerName,
                TotalScans = s.Total,
                InvalidScans = s.Invalid,
                InvalidRatePercent = Math.Round((double)s.Invalid / s.Total * 100, 2),
            };
        }).ToList();

        var topCodes = await scans
            .GroupBy(v => new { v.ScannedCode, v.QRCodeId })
            .Select(g => new
            {
                g.Key.ScannedCode,
                g.Key.QRCodeId,
                Total = g.Count(),
                Invalid = g.Count(v => !v.IsValid),
            })
            .OrderByDescending(x => x.Total)
            .Take(topN)
            .ToListAsync(cancellationToken);

        var codeQrIds = topCodes.Where(c => c.QRCodeId != null).Select(c => c.QRCodeId!.Value).ToList();
        var codeMeta = await _context.QRCodes
            .Where(c => codeQrIds.Contains(c.Id))
            .Select(c => new { c.Id, c.IsActive, c.ProductId, c.Product.Name })
            .ToListAsync(cancellationToken);

        var mostScanned = topCodes.Select(c =>
        {
            var meta = c.QRCodeId != null ? codeMeta.FirstOrDefault(m => m.Id == c.QRCodeId.Value) : null;
            return new QrCodeStatDto
            {
                QrCodeId = c.QRCodeId,
                ScannedCode = c.ScannedCode,
                ProductId = meta?.ProductId,
                ProductName = meta?.Name,
                IsActive = meta?.IsActive ?? false,
                TotalScans = c.Total,
                InvalidScans = c.Invalid,
            };
        }).ToList();

        return new QrMonitoringOverviewDto
        {
            GeneratedAt = DateTime.UtcNow,
            FromDate = from,
            ToDate = to,
            TotalCodes = totalCodes,
            ActiveCodes = activeCodes,
            TotalScans = totalScans,
            ValidScans = validScans,
            InvalidScans = totalScans - validScans,
            UnresolvedScans = unresolved,
            UniqueScanners = uniqueScanners,
            InvalidScanRatePercent = totalScans == 0
                ? 0
                : Math.Round((double)(totalScans - validScans) / totalScans * 100, 2),
            AnomalousProducts = anomalousProducts,
            MostScannedCodes = mostScanned,
        };
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    private static bool TryEnum<T>(string? value, out T result) where T : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out result))
        {
            return true;
        }

        result = default;
        return false;
    }
}
