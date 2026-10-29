using FluentValidation;
using FluentValidation.Results;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.PerformingArts;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Application.Services.PerformingArts;

/// <summary>Performing-arts heritage records. Lifecycle: Draft → InReview → Published → Archived (→ Draft on restore);
/// a reviewer can return an InReview record to Draft. Nothing reaches the public or the AI assistant unless it is
/// Published, and a record cannot be submitted or published without at least one cited source.</summary>
public class PerformingArtService : IPerformingArtService
{
    private const int MaxPageSize = 50;
    private readonly IPerformingArtRepository _repository;

    public PerformingArtService(IPerformingArtRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<PerformingArtListItemDto>> SearchPublishedAsync(PerformingArtQueryParameters query, CancellationToken cancellationToken)
        => SearchAsync(query, publishedOnly: true, cancellationToken);

    public Task<PagedResult<PerformingArtListItemDto>> SearchAdminAsync(PerformingArtQueryParameters query, CancellationToken cancellationToken)
        => SearchAsync(query, publishedOnly: false, cancellationToken);

    public async Task<List<PerformingArtListItemDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken)
    {
        var filter = PerformingArtRules.BuildFilter(new PerformingArtQueryParameters { Featured = true }, publishedOnly: true);
        var (items, _) = await _repository.SearchAsync(filter, 1, Math.Clamp(count, 1, 12), cancellationToken);
        return items.Select(ToListItem).ToList();
    }

    public async Task<PerformingArtFacetsDto> GetFacetsAsync(CancellationToken cancellationToken)
    {
        var records = await _repository.ListAsync(PerformingArtRules.BuildFilter(new PerformingArtQueryParameters(), publishedOnly: true), cancellationToken);
        return new PerformingArtFacetsDto
        {
            Categories = records.GroupBy(r => r.Category).OrderBy(g => g.Key)
                .Select(g => new PerformingArtCategoryFacet(g.Key, g.Count())).ToList(),
            Divisions = records.Where(r => r.Division != null).GroupBy(r => r.Division!).OrderBy(g => g.Key)
                .Select(g => new PerformingArtDivisionFacet(g.Key, g.Count())).ToList(),
            Districts = records.Where(r => r.District != null).GroupBy(r => r.District!.Id).Select(g => g.First().District!)
                .OrderBy(d => d.Name)
                .Select(d => new PerformingArtDistrictFacet(d.Id, d.Name, d.Division, records.Count(r => r.DistrictId == d.Id))).ToList(),
        };
    }

    public async Task<PerformingArtDetailDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var record = await _repository.GetBySlugAsync(PerformingArtRules.NormaliseSlug(slug), cancellationToken);
        if (record is null || record.Status != PerformingArtStatus.Published)
        {
            throw new NotFoundException("Performing arts record not found.");
        }

        return ToDetail(record);
    }

    public async Task<PerformingArtDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => ToDetail(await LoadAsync(id, cancellationToken));

    public async Task<PerformingArtDetailDto> CreateAsync(SavePerformingArtRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var slug = PerformingArtRules.NormaliseSlug(request.Slug);
        await EnsureSlugFreeAsync(slug, null, cancellationToken);

        var now = DateTime.UtcNow;
        var record = new PerformingArtRecord
        {
            Id = Guid.NewGuid(),
            Status = PerformingArtStatus.Draft,
            CreatedByUserId = userId,
            CreatedAt = now,
        };
        await ApplyAsync(record, request, slug, now, cancellationToken);

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDetail(record);
    }

    public async Task<PerformingArtDetailDto> UpdateAsync(Guid id, SavePerformingArtRequest request, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        if (record.Status == PerformingArtStatus.Archived)
        {
            throw new ConflictException("Archived records are read-only. Restore the record to a draft before editing.");
        }

        if ((record.Status is PerformingArtStatus.Published or PerformingArtStatus.InReview) && request.Sources.Count == 0)
        {
            throw new ConflictException("A record that is in review or published must keep at least one source reference.");
        }

        var slug = PerformingArtRules.NormaliseSlug(request.Slug);
        await EnsureSlugFreeAsync(slug, id, cancellationToken);
        await ApplyAsync(record, request, slug, DateTime.UtcNow, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDetail(record);
    }

    public Task<PerformingArtDetailDto> SubmitForReviewAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken)
        => TransitionAsync(id, PerformingArtStatus.InReview, new[] { PerformingArtStatus.Draft }, cancellationToken, record =>
        {
            RequireSources(record, "submitted for review");
            record.SubmittedAt = DateTime.UtcNow;
            record.ReviewNote = Clean(note);
        });

    public Task<PerformingArtDetailDto> PublishAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken)
        => TransitionAsync(id, PerformingArtStatus.Published, new[] { PerformingArtStatus.InReview }, cancellationToken, record =>
        {
            RequireSources(record, "published");
            record.ReviewedByUserId = userId;
            record.PublishedAt = DateTime.UtcNow;
            record.ArchivedAt = null;
            record.ReviewNote = Clean(note);
        });

    public Task<PerformingArtDetailDto> ReturnToDraftAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken)
        => TransitionAsync(id, PerformingArtStatus.Draft, new[] { PerformingArtStatus.InReview }, cancellationToken, record =>
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                throw new ValidationException(new[] { new ValidationFailure("Note", "Explain what needs to change before returning a record to draft.") });
            }

            record.ReviewedByUserId = userId;
            record.ReviewNote = note.Trim();
        });

    public Task<PerformingArtDetailDto> ArchiveAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken)
        => TransitionAsync(id, PerformingArtStatus.Archived,
            new[] { PerformingArtStatus.Draft, PerformingArtStatus.InReview, PerformingArtStatus.Published }, cancellationToken, record =>
            {
                record.ArchivedAt = DateTime.UtcNow;
                record.IsFeatured = false;
                record.ReviewNote = Clean(note) ?? record.ReviewNote;
            });

    public Task<PerformingArtDetailDto> RestoreAsync(Guid id, Guid userId, string? note, CancellationToken cancellationToken)
        => TransitionAsync(id, PerformingArtStatus.Draft, new[] { PerformingArtStatus.Archived }, cancellationToken, record =>
        {
            record.ArchivedAt = null;
            record.PublishedAt = null;
            record.ReviewNote = Clean(note);
        });

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await LoadAsync(id, cancellationToken);
        if (record.Status is PerformingArtStatus.Published or PerformingArtStatus.InReview)
        {
            throw new ConflictException("Only draft or archived records can be deleted. Archive a published record first.");
        }

        _repository.Remove(record);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResult<PerformingArtListItemDto>> SearchAsync(PerformingArtQueryParameters query, bool publishedOnly, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var (items, total) = await _repository.SearchAsync(PerformingArtRules.BuildFilter(query, publishedOnly), page, pageSize, cancellationToken);
        return new PagedResult<PerformingArtListItemDto>
        {
            Items = items.Select(ToListItem).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    private async Task<PerformingArtDetailDto> TransitionAsync(
        Guid id, PerformingArtStatus target, PerformingArtStatus[] allowedFrom, CancellationToken cancellationToken, Action<PerformingArtRecord> apply)
    {
        var record = await LoadAsync(id, cancellationToken);
        if (!allowedFrom.Contains(record.Status))
        {
            throw new ConflictException($"A {record.Status} record cannot be moved to {target}.");
        }

        apply(record);
        record.Status = target;
        record.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDetail(record);
    }

    private static void RequireSources(PerformingArtRecord record, string action)
    {
        if (record.Sources.Count == 0)
        {
            throw new ConflictException($"Add at least one source reference before this record can be {action}.");
        }
    }

    private async Task EnsureSlugFreeAsync(string slug, Guid? exceptId, CancellationToken cancellationToken)
    {
        if (await _repository.SlugExistsAsync(slug, exceptId, cancellationToken))
        {
            throw new ConflictException("A performing arts record with this slug already exists.");
        }
    }

    private async Task<PerformingArtRecord> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Performing arts record not found.");

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task ApplyAsync(PerformingArtRecord r, SavePerformingArtRequest q, string slug, DateTime now, CancellationToken cancellationToken)
    {
        var division = Clean(q.Division);
        if (q.DistrictId is { } districtId)
        {
            var district = await _repository.GetDistrictAsync(districtId, cancellationToken)
                ?? throw new NotFoundException("District not found.");
            r.District = district;
            division = district.Division; // a district always fixes its division, so the two can never disagree
        }
        else
        {
            r.District = null;
        }

        r.Slug = slug;
        r.TitleEn = q.TitleEn.Trim();
        r.TitleBn = Clean(q.TitleBn);
        r.AlternateNames = Clean(q.AlternateNames);
        r.Category = q.Category;
        r.SummaryEn = q.SummaryEn.Trim();
        r.SummaryBn = Clean(q.SummaryBn);
        r.DescriptionEn = Clean(q.DescriptionEn);
        r.DescriptionBn = Clean(q.DescriptionBn);
        r.RegionalContextEn = Clean(q.RegionalContextEn);
        r.RegionalContextBn = Clean(q.RegionalContextBn);
        r.Division = division;
        r.DistrictId = q.DistrictId;
        r.Upazila = Clean(q.Upazila);
        r.Communities = Clean(q.Communities);
        r.Artists = Clean(q.Artists);
        r.Instruments = Clean(q.Instruments);
        r.ViabilityNote = Clean(q.ViabilityNote);
        r.InventoryReference = Clean(q.InventoryReference);
        r.IsFeatured = q.IsFeatured && r.Status != PerformingArtStatus.Archived;
        r.DisplayOrder = q.DisplayOrder;
        r.UpdatedAt = now;

        // Children are replaced wholesale; new rows keep Guid.Empty keys so EF treats them as inserts.
        r.Sources.Clear();
        foreach (var (s, i) in q.Sources.Select((s, i) => (s, i)))
        {
            r.Sources.Add(new PerformingArtSource
            {
                RecordId = r.Id, SourceType = s.SourceType, Title = s.Title.Trim(), Publisher = Clean(s.Publisher),
                Url = Clean(s.Url), AccessedOn = s.AccessedOn, Notes = Clean(s.Notes), DisplayOrder = i,
            });
        }

        r.Media.Clear();
        foreach (var (m, i) in q.Media.Select((m, i) => (m, i)))
        {
            r.Media.Add(new PerformingArtMedia
            {
                RecordId = r.Id, MediaType = m.MediaType, Url = m.Url.Trim(), CaptionEn = Clean(m.CaptionEn), CaptionBn = Clean(m.CaptionBn),
                Credit = m.Credit.Trim(), License = Clean(m.License), SourceUrl = Clean(m.SourceUrl), DisplayOrder = i,
            });
        }
    }

    internal static PerformingArtListItemDto ToListItem(PerformingArtRecord r)
    {
        var cover = r.Media.Where(m => m.MediaType == PerformingArtMediaType.Image).OrderBy(m => m.DisplayOrder).FirstOrDefault();
        return new PerformingArtListItemDto
        {
            Id = r.Id, Slug = r.Slug, TitleEn = r.TitleEn, TitleBn = r.TitleBn, Category = r.Category, SummaryEn = r.SummaryEn,
            SummaryBn = r.SummaryBn, Division = r.Division, DistrictId = r.DistrictId, DistrictName = r.District?.Name,
            Communities = r.Communities, IsFeatured = r.IsFeatured, CoverImageUrl = cover?.Url, CoverImageCredit = cover?.Credit,
            SourceCount = r.Sources.Count, Status = r.Status, UpdatedAt = r.UpdatedAt,
        };
    }

    internal static PerformingArtDetailDto ToDetail(PerformingArtRecord r) => new()
    {
        Id = r.Id, Slug = r.Slug, TitleEn = r.TitleEn, TitleBn = r.TitleBn, AlternateNames = r.AlternateNames, Category = r.Category,
        SummaryEn = r.SummaryEn, SummaryBn = r.SummaryBn, DescriptionEn = r.DescriptionEn, DescriptionBn = r.DescriptionBn,
        RegionalContextEn = r.RegionalContextEn, RegionalContextBn = r.RegionalContextBn, Division = r.Division,
        DistrictId = r.DistrictId, DistrictName = r.District?.Name, Upazila = r.Upazila, Communities = r.Communities,
        Artists = r.Artists, Instruments = r.Instruments, ViabilityNote = r.ViabilityNote, InventoryReference = r.InventoryReference,
        IsFeatured = r.IsFeatured, DisplayOrder = r.DisplayOrder, Status = r.Status, ReviewNote = r.ReviewNote,
        SubmittedAt = r.SubmittedAt, PublishedAt = r.PublishedAt, ArchivedAt = r.ArchivedAt, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt,
        Sources = r.Sources.OrderBy(s => s.DisplayOrder).Select(s => new PerformingArtSourceDto
        {
            Id = s.Id, SourceType = s.SourceType, Title = s.Title, Publisher = s.Publisher, Url = s.Url,
            AccessedOn = s.AccessedOn, Notes = s.Notes, DisplayOrder = s.DisplayOrder,
        }).ToList(),
        Media = r.Media.OrderBy(m => m.DisplayOrder).Select(m => new PerformingArtMediaDto
        {
            Id = m.Id, MediaType = m.MediaType, Url = m.Url, CaptionEn = m.CaptionEn, CaptionBn = m.CaptionBn,
            Credit = m.Credit, License = m.License, SourceUrl = m.SourceUrl, DisplayOrder = m.DisplayOrder,
        }).ToList(),
    };
}
