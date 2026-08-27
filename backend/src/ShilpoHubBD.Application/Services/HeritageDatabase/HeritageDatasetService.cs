using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Services.HeritageDatabase;

public class HeritageDatasetService : IHeritageDatasetService
{
    private readonly IHeritageDatasetRepository _repository;
    private readonly IHeritageDataRepository _dataRepository;
    private readonly IUserRepository _userRepository;

    public HeritageDatasetService(
        IHeritageDatasetRepository repository,
        IHeritageDataRepository dataRepository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _dataRepository = dataRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<HeritageDatasetListItemDto>> GetAccessibleAsync(
        HeritageDbAccessContext ctx, HeritageDatasetQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAccessibleAsync(
            ctx.UserId, ctx.IsResearcher || ctx.IsSuperAdmin, query, cancellationToken);

        return new PagedResult<HeritageDatasetListItemDto>
        {
            Items = items.Select(d => d.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritageDatasetDetailDto> GetByIdAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetDetailAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        var canManage = CanManage(dataset, ctx, grant);
        if (!CanRead(dataset, ctx, grant))
        {
            throw new NotFoundException("Dataset not found.");
        }

        if (dataset.Status == HeritageDatasetStatus.Draft && !canManage)
        {
            throw new NotFoundException("Dataset not found.");
        }

        return ToDetailDto(dataset, ctx, grant, canManage);
    }

    public async Task<HeritageDatasetDetailDto> CreateAsync(
        HeritageDbAccessContext ctx, CreateHeritageDatasetRequest request, CancellationToken cancellationToken)
    {
        var category = ParseCategory(request.Category);
        var accessLevel = ParseEnum<HeritageDatasetAccessLevel>(request.AccessLevel) ?? HeritageDatasetAccessLevel.Researcher;
        var sourceType = ParseEnum<HeritageDatasetSourceType>(request.SourceType) ?? HeritageDatasetSourceType.PlatformLive;

        var now = DateTime.UtcNow;
        var dataset = new HeritageDataset
        {
            Id = Guid.NewGuid(),
            Slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Category = category,
            Status = HeritageDatasetStatus.Draft,
            AccessLevel = accessLevel,
            SourceType = sourceType,
            SourceOrganization = request.SourceOrganization?.Trim(),
            SourceReference = request.SourceReference?.Trim(),
            License = request.License?.Trim(),
            Tags = NormalizeTags(request.Tags),
            IsLive = request.IsLive,
            OwnerUserId = ctx.UserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (dataset.IsLive)
        {
            dataset.RecordCount = await _dataRepository.CountLiveRecordsAsync(category, cancellationToken);
            dataset.LastRefreshedAt = now;
            dataset.DataUpdatedAt = now;
        }

        await _repository.AddAsync(dataset, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(ctx, dataset.Id, cancellationToken);
    }

    public async Task<HeritageDatasetDetailDto> UpdateAsync(
        HeritageDbAccessContext ctx, Guid datasetId, UpdateHeritageDatasetRequest request, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetDetailAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, grant);

        var category = ParseCategory(request.Category);
        var status = ParseEnum<HeritageDatasetStatus>(request.Status)
            ?? throw new ConflictException("Status must be one of: Draft, Published, Archived, Deprecated.");
        var accessLevel = ParseEnum<HeritageDatasetAccessLevel>(request.AccessLevel)
            ?? throw new ConflictException("AccessLevel must be one of: Public, Researcher, Restricted.");
        var sourceType = ParseEnum<HeritageDatasetSourceType>(request.SourceType)
            ?? throw new ConflictException("SourceType must be a valid source type.");

        dataset.Name = request.Name.Trim();
        dataset.Description = request.Description.Trim();
        dataset.Category = category;
        dataset.Status = status;
        dataset.AccessLevel = accessLevel;
        dataset.SourceType = sourceType;
        dataset.SourceOrganization = request.SourceOrganization?.Trim();
        dataset.SourceReference = request.SourceReference?.Trim();
        dataset.License = request.License?.Trim();
        dataset.Tags = NormalizeTags(request.Tags);
        dataset.IsLive = request.IsLive;
        dataset.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(ctx, dataset.Id, cancellationToken);
    }

    public async Task<HeritageDatasetDetailDto> RefreshAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetDetailAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, grant);

        if (!dataset.IsLive)
        {
            throw new ConflictException("Only live datasets can be refreshed from platform data.");
        }

        var now = DateTime.UtcNow;
        dataset.RecordCount = await _dataRepository.CountLiveRecordsAsync(dataset.Category, cancellationToken);
        dataset.LastRefreshedAt = now;
        dataset.DataUpdatedAt = now;
        dataset.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(ctx, dataset.Id, cancellationToken);
    }

    public async Task DeleteAsync(HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        if (dataset.OwnerUserId != ctx.UserId && !ctx.IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Only the dataset owner can delete it.");
        }

        _repository.Remove(dataset);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<HeritageDatasetVersionDto>> GetVersionsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        await LoadReadableAsync(ctx, datasetId, cancellationToken);
        var versions = await _repository.GetVersionsAsync(datasetId, cancellationToken);
        return versions.Select(v => v.ToDto()).ToList();
    }

    public async Task<HeritageDatasetVersionDto> AddVersionAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CreateHeritageDatasetVersionRequest request, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetDetailAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, grant);

        var format = ParseEnum<HeritageDatasetFileFormat>(request.Format) ?? HeritageDatasetFileFormat.None;
        var nextNumber = await _repository.GetMaxVersionNumberAsync(datasetId, cancellationToken) + 1;

        var recordCount = request.RecordCount
            ?? (dataset.IsLive
                ? await _dataRepository.CountLiveRecordsAsync(dataset.Category, cancellationToken)
                : dataset.RecordCount);

        var now = DateTime.UtcNow;
        var version = new HeritageDatasetVersion
        {
            Id = Guid.NewGuid(),
            HeritageDatasetId = datasetId,
            VersionNumber = nextNumber,
            Label = string.IsNullOrWhiteSpace(request.Label) ? $"v{nextNumber}" : request.Label.Trim(),
            Changelog = request.Changelog.Trim(),
            RecordCount = recordCount,
            Format = format,
            SourceFileName = request.SourceFileName?.Trim(),
            SourceFileUrl = request.SourceFileUrl?.Trim(),
            SourceContentHash = request.SourceContentHash?.Trim(),
            ImportedRowCount = request.ImportedRowCount,
            ImportErrorCount = request.ImportErrorCount < 0 ? 0 : request.ImportErrorCount,
            ImportNotes = request.ImportNotes?.Trim(),
            SchemaJson = request.SchemaJson,
            IsCurrent = request.SetAsCurrent,
            CreatedByUserId = ctx.UserId,
            CreatedAt = now,
            PublishedAt = request.Publish ? now : null,
        };

        if (request.SetAsCurrent)
        {
            foreach (var existing in dataset.Versions.Where(v => v.IsCurrent))
            {
                existing.IsCurrent = false;
            }
        }

        await _repository.AddVersionAsync(version, cancellationToken);

        dataset.VersionCount += 1;
        dataset.DataUpdatedAt = now;
        dataset.UpdatedAt = now;
        if (request.SetAsCurrent)
        {
            dataset.CurrentVersionId = version.Id;
            dataset.RecordCount = recordCount;
        }

        if (request.Publish && dataset.Status == HeritageDatasetStatus.Draft)
        {
            dataset.Status = HeritageDatasetStatus.Published;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetVersionByIdAsync(version.Id, cancellationToken))!.ToDto();
    }

    public async Task<List<HeritageDatasetAccessGrantDto>> GetAccessGrantsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, grant);

        var grants = await _repository.GetGrantsAsync(datasetId, cancellationToken);
        return grants.Select(g => g.ToDto()).ToList();
    }

    public async Task<HeritageDatasetAccessGrantDto> GrantAccessAsync(
        HeritageDbAccessContext ctx, Guid datasetId, GrantHeritageDatasetAccessRequest request, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var callerGrant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, callerGrant);

        var accessRole = ParseEnum<HeritageDatasetAccessRole>(request.AccessRole)
            ?? throw new ConflictException("AccessRole must be one of: Viewer, Analyst, Maintainer.");

        if (request.UserId == dataset.OwnerUserId)
        {
            throw new ConflictException("The dataset owner already has full access.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
        {
            throw new ConflictException("ExpiresAt must be in the future.");
        }

        var existing = await _repository.GetGrantAsync(datasetId, request.UserId, cancellationToken);
        if (existing is not null)
        {
            existing.AccessRole = accessRole;
            existing.ExpiresAt = request.ExpiresAt;
            existing.GrantedByUserId = ctx.UserId;
            existing.GrantedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);
            return existing.ToDto();
        }

        var grant = new HeritageDatasetAccessGrant
        {
            Id = Guid.NewGuid(),
            HeritageDatasetId = datasetId,
            UserId = request.UserId,
            AccessRole = accessRole,
            GrantedByUserId = ctx.UserId,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
        };

        await _repository.AddGrantAsync(grant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        grant.User = user;
        return grant.ToDto();
    }

    public async Task RevokeAccessAsync(
        HeritageDbAccessContext ctx, Guid datasetId, Guid grantId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var callerGrant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, callerGrant);

        var grant = await _repository.GetGrantByIdAsync(grantId, cancellationToken);
        if (grant is null || grant.HeritageDatasetId != datasetId)
        {
            throw new NotFoundException("Access grant not found.");
        }

        _repository.RemoveGrant(grant);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<HeritageDatasetExportDto> CreateExportAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CreateHeritageDatasetExportRequest request, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetDetailAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        if (!CanRead(dataset, ctx, grant))
        {
            throw new NotFoundException("Dataset not found.");
        }

        var format = ParseEnum<HeritageDatasetFileFormat>(request.Format) ?? HeritageDatasetFileFormat.Csv;

        HeritageDatasetVersion? version = null;
        if (request.DatasetVersionId.HasValue)
        {
            version = dataset.Versions.FirstOrDefault(v => v.Id == request.DatasetVersionId.Value)
                ?? throw new NotFoundException("Dataset version not found.");
        }

        var rowCount = version?.RecordCount
            ?? (dataset.IsLive
                ? await _dataRepository.CountLiveRecordsAsync(dataset.Category, cancellationToken)
                : dataset.RecordCount);

        var now = DateTime.UtcNow;
        var export = new HeritageDatasetExport
        {
            Id = Guid.NewGuid(),
            HeritageDatasetId = datasetId,
            DatasetVersionId = version?.Id,
            RequestedByUserId = ctx.UserId,
            Format = format,
            RowCount = rowCount,
            FilterJson = request.FilterJson,
            Notes = request.Notes?.Trim(),
            // The backend records export metadata only; an external worker produces the file.
            Status = HeritageDatasetExportStatus.Completed,
            FileUrl = null,
            CreatedAt = now,
            CompletedAt = now,
        };

        await _repository.AddExportAsync(export, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetExportByIdAsync(export.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<HeritageDatasetExportDto>> GetExportsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, grant);

        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetExportsForDatasetAsync(datasetId, query, cancellationToken);
        return new PagedResult<HeritageDatasetExportDto>
        {
            Items = items.Select(e => e.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritageDatasetExportAnalyticsDto> GetExportAnalyticsAsync(
        HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        RequireManage(dataset, ctx, grant);

        return await _repository.GetExportAnalyticsAsync(datasetId, cancellationToken);
    }

    public async Task<PagedResult<HeritageDatasetExportDto>> GetMyExportsAsync(
        Guid userId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetExportsForUserAsync(userId, query, cancellationToken);
        return new PagedResult<HeritageDatasetExportDto>
        {
            Items = items.Select(e => e.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    // ---- access helpers -------------------------------------------------

    private async Task LoadReadableAsync(HeritageDbAccessContext ctx, Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await _repository.GetByIdAsync(datasetId, cancellationToken)
            ?? throw new NotFoundException("Dataset not found.");

        var grant = dataset.AccessGrants.FirstOrDefault(g => g.UserId == ctx.UserId);
        if (!CanRead(dataset, ctx, grant))
        {
            throw new NotFoundException("Dataset not found.");
        }
    }

    private static bool CanRead(HeritageDataset dataset, HeritageDbAccessContext ctx, HeritageDatasetAccessGrant? grant)
    {
        if (dataset.OwnerUserId == ctx.UserId || ctx.IsSuperAdmin)
        {
            return true;
        }

        if (grant is not null && (grant.ExpiresAt is null || grant.ExpiresAt > DateTime.UtcNow))
        {
            return true;
        }

        return dataset.AccessLevel switch
        {
            HeritageDatasetAccessLevel.Public => true,
            HeritageDatasetAccessLevel.Researcher => ctx.IsResearcher,
            _ => false,
        };
    }

    private static bool CanManage(HeritageDataset dataset, HeritageDbAccessContext ctx, HeritageDatasetAccessGrant? grant)
    {
        if (dataset.OwnerUserId == ctx.UserId || ctx.IsSuperAdmin)
        {
            return true;
        }

        return grant is not null
            && grant.AccessRole == HeritageDatasetAccessRole.Maintainer
            && (grant.ExpiresAt is null || grant.ExpiresAt > DateTime.UtcNow);
    }

    private static void RequireManage(HeritageDataset dataset, HeritageDbAccessContext ctx, HeritageDatasetAccessGrant? grant)
    {
        if (!CanRead(dataset, ctx, grant))
        {
            throw new NotFoundException("Dataset not found.");
        }

        if (!CanManage(dataset, ctx, grant))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this dataset.");
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(name);
        var slug = baseSlug;
        var suffix = 1;

        while (await _repository.SlugExistsAsync(slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static string? NormalizeTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return null;
        }

        var parts = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .ToList();

        return parts.Count == 0 ? null : string.Join(",", parts);
    }

    private static HeritageDatasetCategory ParseCategory(string value)
        => Enum.TryParse<HeritageDatasetCategory>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("Category must be a valid dataset category.");

    private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : null;
    }

    private static HeritageDatasetDetailDto ToDetailDto(
        HeritageDataset dataset, HeritageDbAccessContext ctx, HeritageDatasetAccessGrant? grant, bool canManage)
    {
        var myAccess = dataset.OwnerUserId == ctx.UserId
            ? "Owner"
            : grant?.AccessRole.ToString() ?? (canManage ? "Manager" : "Reader");

        return new HeritageDatasetDetailDto
        {
            Id = dataset.Id,
            Slug = dataset.Slug,
            Name = dataset.Name,
            Description = dataset.Description,
            Category = dataset.Category.ToString(),
            Status = dataset.Status.ToString(),
            AccessLevel = dataset.AccessLevel.ToString(),
            SourceType = dataset.SourceType.ToString(),
            SourceOrganization = dataset.SourceOrganization,
            SourceReference = dataset.SourceReference,
            License = dataset.License,
            Tags = dataset.Tags,
            IsLive = dataset.IsLive,
            RecordCount = dataset.RecordCount,
            VersionCount = dataset.VersionCount,
            CurrentVersionId = dataset.CurrentVersionId,
            OwnerUserId = dataset.OwnerUserId,
            OwnerName = dataset.Owner?.FullName ?? string.Empty,
            MyAccess = myAccess,
            CanManage = canManage,
            DataUpdatedAt = dataset.DataUpdatedAt,
            LastRefreshedAt = dataset.LastRefreshedAt,
            CreatedAt = dataset.CreatedAt,
            UpdatedAt = dataset.UpdatedAt,
            Versions = dataset.Versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => v.ToDto())
                .ToList(),
            AccessGrants = canManage
                ? dataset.AccessGrants.OrderBy(g => g.GrantedAt).Select(g => g.ToDto()).ToList()
                : new List<HeritageDatasetAccessGrantDto>(),
        };
    }
}
