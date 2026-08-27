using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Services.HeritageDatabase;

internal static class HeritageDatabaseMappings
{
    public static HeritageDatasetListItemDto ToListItemDto(this HeritageDataset d) => new()
    {
        Id = d.Id,
        Slug = d.Slug,
        Name = d.Name,
        Category = d.Category.ToString(),
        Status = d.Status.ToString(),
        AccessLevel = d.AccessLevel.ToString(),
        SourceType = d.SourceType.ToString(),
        Tags = d.Tags,
        IsLive = d.IsLive,
        RecordCount = d.RecordCount,
        VersionCount = d.VersionCount,
        OwnerName = d.Owner?.FullName ?? string.Empty,
        DataUpdatedAt = d.DataUpdatedAt,
        LastRefreshedAt = d.LastRefreshedAt,
        UpdatedAt = d.UpdatedAt,
    };

    public static HeritageDatasetVersionDto ToDto(this HeritageDatasetVersion v) => new()
    {
        Id = v.Id,
        HeritageDatasetId = v.HeritageDatasetId,
        VersionNumber = v.VersionNumber,
        Label = v.Label,
        Changelog = v.Changelog,
        RecordCount = v.RecordCount,
        Format = v.Format.ToString(),
        SourceFileName = v.SourceFileName,
        SourceFileUrl = v.SourceFileUrl,
        SourceContentHash = v.SourceContentHash,
        ImportedRowCount = v.ImportedRowCount,
        ImportErrorCount = v.ImportErrorCount,
        ImportNotes = v.ImportNotes,
        SchemaJson = v.SchemaJson,
        IsCurrent = v.IsCurrent,
        CreatedByUserId = v.CreatedByUserId,
        CreatedByName = v.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = v.CreatedAt,
        PublishedAt = v.PublishedAt,
    };

    public static HeritageDatasetAccessGrantDto ToDto(this HeritageDatasetAccessGrant g) => new()
    {
        Id = g.Id,
        HeritageDatasetId = g.HeritageDatasetId,
        UserId = g.UserId,
        UserName = g.User?.FullName ?? string.Empty,
        UserEmail = g.User?.Email ?? string.Empty,
        AccessRole = g.AccessRole.ToString(),
        GrantedByUserId = g.GrantedByUserId,
        GrantedAt = g.GrantedAt,
        ExpiresAt = g.ExpiresAt,
    };

    public static HeritageDatasetExportDto ToDto(this HeritageDatasetExport e) => new()
    {
        Id = e.Id,
        HeritageDatasetId = e.HeritageDatasetId,
        DatasetName = e.Dataset?.Name ?? string.Empty,
        DatasetVersionId = e.DatasetVersionId,
        VersionNumber = e.Version?.VersionNumber,
        RequestedByUserId = e.RequestedByUserId,
        RequestedByName = e.RequestedBy?.FullName ?? string.Empty,
        Format = e.Format.ToString(),
        RowCount = e.RowCount,
        FilterJson = e.FilterJson,
        Notes = e.Notes,
        Status = e.Status.ToString(),
        FileUrl = e.FileUrl,
        CreatedAt = e.CreatedAt,
        CompletedAt = e.CompletedAt,
    };

    public static HeritageRiskRecordDto ToDto(this HeritageRiskRecord r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        Category = r.Category.ToString(),
        Level = r.Level.ToString(),
        CraftName = r.CraftName,
        DistrictId = r.DistrictId,
        DistrictName = r.District?.Name,
        VillageId = r.VillageId,
        VillageName = r.Village?.Name,
        ProducerId = r.ProducerId,
        ProducerName = r.Producer?.FullName,
        AffectedArtisanCount = r.AffectedArtisanCount,
        ContributingFactors = r.ContributingFactors,
        RecommendedActions = r.RecommendedActions,
        Source = r.Source,
        AssessmentYear = r.AssessmentYear,
        AssessedOn = r.AssessedOn,
        CreatedByUserId = r.CreatedByUserId,
        CreatedByName = r.CreatedBy?.FullName ?? string.Empty,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}
