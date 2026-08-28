using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

internal static class GovernanceMappings
{
    public static NationalDashboardSnapshotDto ToDto(this NationalDashboardSnapshot s) => new()
    {
        Id = s.Id,
        Label = s.Label,
        Period = s.Period.ToString(),
        PeriodStart = s.PeriodStart,
        PeriodEnd = s.PeriodEnd,
        CapturedAt = s.CapturedAt,
        TotalProducers = s.TotalProducers,
        ActiveProducers = s.ActiveProducers,
        VerifiedHeritageProducers = s.VerifiedHeritageProducers,
        NewProducers = s.NewProducers,
        JobsPosted = s.JobsPosted,
        JobApplications = s.JobApplications,
        JobsFilled = s.JobsFilled,
        ExporterPartners = s.ExporterPartners,
        ExportOrders = s.ExportOrders,
        ExportSalesValue = s.ExportSalesValue,
        TotalOrders = s.TotalOrders,
        ProductsSold = s.ProductsSold,
        MarketplaceSalesValue = s.MarketplaceSalesValue,
        HeritageEconomyValue = s.HeritageEconomyValue,
        TourismBookings = s.TourismBookings,
        TourismRevenue = s.TourismRevenue,
        TouristsServed = s.TouristsServed,
        DistrictsCovered = s.DistrictsCovered,
        VillagesCovered = s.VillagesCovered,
        ProductsListed = s.ProductsListed,
        Notes = s.Notes,
        GeneratedByUserId = s.GeneratedByUserId,
        GeneratedByName = s.GeneratedBy?.FullName,
        CreatedAt = s.CreatedAt,
        DistrictStats = s.DistrictStats
            .OrderBy(d => d.Rank)
            .Select(d => new DashboardDistrictStatDto
            {
                DistrictId = d.DistrictId,
                DistrictName = d.DistrictName,
                Division = d.Division,
                ProducerCount = d.ProducerCount,
                ProductCount = d.ProductCount,
                VillageCount = d.VillageCount,
                OrderCount = d.OrderCount,
                SalesValue = d.SalesValue,
                Rank = d.Rank,
            })
            .ToList(),
    };

    public static NationalDashboardSnapshotListItemDto ToListItemDto(this NationalDashboardSnapshot s) => new()
    {
        Id = s.Id,
        Label = s.Label,
        Period = s.Period.ToString(),
        PeriodStart = s.PeriodStart,
        PeriodEnd = s.PeriodEnd,
        CapturedAt = s.CapturedAt,
        TotalProducers = s.TotalProducers,
        HeritageEconomyValue = s.HeritageEconomyValue,
        ExportSalesValue = s.ExportSalesValue,
        GeneratedByName = s.GeneratedBy?.FullName,
    };

    public static HeritageIndexRecordDto ToDto(this HeritageIndexRecord r) => new()
    {
        Id = r.Id,
        IndexType = r.IndexType.ToString(),
        Scope = r.Scope.ToString(),
        ScopeId = r.ScopeId,
        ScopeLabel = r.ScopeLabel,
        Score = r.Score,
        Rating = r.Rating.ToString(),
        Method = r.Method,
        Summary = r.Summary,
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        ComputedAt = r.ComputedAt,
        Notes = r.Notes,
        GeneratedByUserId = r.GeneratedByUserId,
        GeneratedByName = r.GeneratedBy?.FullName,
        CreatedAt = r.CreatedAt,
        Components = r.Components
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new HeritageIndexComponentDto
            {
                Key = c.Key,
                Label = c.Label,
                RawValue = c.RawValue,
                Weight = c.Weight,
                ContributionScore = c.ContributionScore,
                Detail = c.Detail,
                DisplayOrder = c.DisplayOrder,
            })
            .ToList(),
    };

    public static HeritageIndexRecordListItemDto ToListItemDto(this HeritageIndexRecord r) => new()
    {
        Id = r.Id,
        IndexType = r.IndexType.ToString(),
        Scope = r.Scope.ToString(),
        ScopeId = r.ScopeId,
        ScopeLabel = r.ScopeLabel,
        Score = r.Score,
        Rating = r.Rating.ToString(),
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        ComputedAt = r.ComputedAt,
        GeneratedByName = r.GeneratedBy?.FullName,
    };

    public static PolicySimulationDto ToDto(this PolicySimulation s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        SimulationType = s.SimulationType.ToString(),
        Scope = s.Scope.ToString(),
        ScopeId = s.ScopeId,
        ScopeLabel = s.ScopeLabel,
        Status = s.Status.ToString(),
        HorizonMonths = s.HorizonMonths,
        InputsJson = s.InputsJson,
        AssumptionsJson = s.AssumptionsJson,
        Method = s.Method,
        Summary = s.Summary,
        Confidence = s.Confidence.ToString(),
        BaselineProducers = s.BaselineProducers,
        BaselineActiveProducers = s.BaselineActiveProducers,
        BaselineEmployment = s.BaselineEmployment,
        BaselineExportValue = s.BaselineExportValue,
        BaselineTourismRevenue = s.BaselineTourismRevenue,
        BaselineEconomyValue = s.BaselineEconomyValue,
        Notes = s.Notes,
        FailureReason = s.FailureReason,
        GeneratedByUserId = s.GeneratedByUserId,
        GeneratedByName = s.GeneratedBy?.FullName,
        CreatedAt = s.CreatedAt,
        CompletedAt = s.CompletedAt,
        Projections = s.Projections
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PolicySimulationProjectionDto
            {
                Metric = p.Metric,
                Unit = p.Unit,
                BaselineValue = p.BaselineValue,
                ProjectedValue = p.ProjectedValue,
                DeltaValue = p.DeltaValue,
                DeltaPercent = p.DeltaPercent,
                HorizonMonths = p.HorizonMonths,
                Confidence = p.Confidence.ToString(),
                Detail = p.Detail,
                DisplayOrder = p.DisplayOrder,
            })
            .ToList(),
        Recommendations = s.Recommendations
            .OrderBy(r => r.DisplayOrder)
            .Select(r => new PolicySimulationRecommendationDto
            {
                Priority = r.Priority.ToString(),
                Title = r.Title,
                Detail = r.Detail,
                DisplayOrder = r.DisplayOrder,
            })
            .ToList(),
    };

    public static PolicySimulationListItemDto ToListItemDto(this PolicySimulation s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        SimulationType = s.SimulationType.ToString(),
        Scope = s.Scope.ToString(),
        ScopeLabel = s.ScopeLabel,
        Status = s.Status.ToString(),
        HorizonMonths = s.HorizonMonths,
        Confidence = s.Confidence.ToString(),
        CreatedAt = s.CreatedAt,
        GeneratedByName = s.GeneratedBy?.FullName,
    };

    // ---- Monitoring flags -------------------------------------------

    public static MonitoringFlagDto ToDto(this MonitoringFlag f) => new()
    {
        Id = f.Id,
        FlagType = f.FlagType.ToString(),
        Severity = f.Severity.ToString(),
        Status = f.Status.ToString(),
        Source = f.Source.ToString(),
        SubjectType = f.SubjectType.ToString(),
        SubjectId = f.SubjectId,
        SubjectLabel = f.SubjectLabel,
        Title = f.Title,
        Description = f.Description,
        EvidenceJson = f.EvidenceJson,
        RiskScore = f.RiskScore,
        DetectedAt = f.DetectedAt,
        AssignedToUserId = f.AssignedToUserId,
        AssignedToName = f.AssignedTo?.FullName,
        ResolvedAt = f.ResolvedAt,
        ResolvedByName = f.ResolvedBy?.FullName,
        ResolutionNotes = f.ResolutionNotes,
        CreatedByUserId = f.CreatedByUserId,
        CreatedByName = f.CreatedBy?.FullName,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
        Events = f.Events
            .OrderBy(e => e.CreatedAt)
            .Select(e => new MonitoringFlagEventDto
            {
                Type = e.Type.ToString(),
                Note = e.Note,
                FromStatus = e.FromStatus?.ToString(),
                ToStatus = e.ToStatus?.ToString(),
                ActorUserId = e.ActorUserId,
                ActorName = e.Actor?.FullName,
                CreatedAt = e.CreatedAt,
            })
            .ToList(),
    };

    public static MonitoringFlagListItemDto ToListItemDto(this MonitoringFlag f) => new()
    {
        Id = f.Id,
        FlagType = f.FlagType.ToString(),
        Severity = f.Severity.ToString(),
        Status = f.Status.ToString(),
        SubjectType = f.SubjectType.ToString(),
        SubjectId = f.SubjectId,
        SubjectLabel = f.SubjectLabel,
        Title = f.Title,
        RiskScore = f.RiskScore,
        DetectedAt = f.DetectedAt,
        AssignedToName = f.AssignedTo?.FullName,
    };

    // ---- Complaints -----------------------------------------------

    public static ComplaintDto ToDto(this Complaint c) => new()
    {
        Id = c.Id,
        ReferenceCode = c.ReferenceCode,
        Category = c.Category.ToString(),
        Status = c.Status.ToString(),
        Priority = c.Priority.ToString(),
        Title = c.Title,
        Description = c.Description,
        ComplainantUserId = c.ComplainantUserId,
        ComplainantName = c.ComplainantName ?? c.ComplainantUser?.FullName,
        ComplainantContact = c.ComplainantContact,
        AgainstType = c.AgainstType.ToString(),
        AgainstId = c.AgainstId,
        AgainstLabel = c.AgainstLabel,
        RelatedOrderId = c.RelatedOrderId,
        MonitoringFlagId = c.MonitoringFlagId,
        AssignedToUserId = c.AssignedToUserId,
        AssignedToName = c.AssignedTo?.FullName,
        Resolution = c.Resolution,
        ResolvedAt = c.ResolvedAt,
        ResolvedByName = c.ResolvedBy?.FullName,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Updates = c.Updates
            .OrderBy(u => u.CreatedAt)
            .Select(u => new ComplaintUpdateDto
            {
                Id = u.Id,
                Message = u.Message,
                IsInternal = u.IsInternal,
                FromStatus = u.FromStatus?.ToString(),
                ToStatus = u.ToStatus?.ToString(),
                ActorUserId = u.ActorUserId,
                ActorName = u.Actor?.FullName,
                CreatedAt = u.CreatedAt,
            })
            .ToList(),
    };

    public static ComplaintListItemDto ToListItemDto(this Complaint c) => new()
    {
        Id = c.Id,
        ReferenceCode = c.ReferenceCode,
        Category = c.Category.ToString(),
        Status = c.Status.ToString(),
        Priority = c.Priority.ToString(),
        Title = c.Title,
        AgainstLabel = c.AgainstLabel,
        AssignedToName = c.AssignedTo?.FullName,
        CreatedAt = c.CreatedAt,
    };

    // ---- Compliance ---------------------------------------------

    public static ComplianceRecordDto ToDto(this ComplianceRecord r) => new()
    {
        Id = r.Id,
        EntityType = r.EntityType.ToString(),
        EntityId = r.EntityId,
        EntityLabel = r.EntityLabel,
        Framework = r.Framework,
        Status = r.Status.ToString(),
        OverallScorePercent = r.OverallScorePercent,
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        LastReviewedAt = r.LastReviewedAt,
        NextReviewDue = r.NextReviewDue,
        ReviewerUserId = r.ReviewerUserId,
        ReviewerName = r.Reviewer?.FullName,
        Notes = r.Notes,
        CreatedByUserId = r.CreatedByUserId,
        CreatedByName = r.CreatedBy?.FullName,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Requirements = r.Requirements
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ComplianceRequirementDto
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Description = x.Description,
                IsMandatory = x.IsMandatory,
                Status = x.Status.ToString(),
                Evidence = x.Evidence,
                ReviewedAt = x.ReviewedAt,
                DisplayOrder = x.DisplayOrder,
            })
            .ToList(),
    };

    public static ComplianceRecordListItemDto ToListItemDto(this ComplianceRecord r) => new()
    {
        Id = r.Id,
        EntityType = r.EntityType.ToString(),
        EntityId = r.EntityId,
        EntityLabel = r.EntityLabel,
        Framework = r.Framework,
        Status = r.Status.ToString(),
        OverallScorePercent = r.OverallScorePercent,
        NextReviewDue = r.NextReviewDue,
        UpdatedAt = r.UpdatedAt,
    };

    // ---- Funding programmes -----------------------------------------

    public static FundingProgramDto ToDto(this FundingProgram p, int applicationCount, int approvedCount) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Type = p.Type.ToString(),
        Status = p.Status.ToString(),
        Description = p.Description,
        EligibilityCriteria = p.EligibilityCriteria,
        Currency = p.Currency,
        TotalBudget = p.TotalBudget,
        AllocatedAmount = p.AllocatedAmount,
        DisbursedAmount = p.DisbursedAmount,
        RemainingBudget = p.TotalBudget - p.AllocatedAmount,
        MinAmountPerApplicant = p.MinAmountPerApplicant,
        MaxAmountPerApplicant = p.MaxAmountPerApplicant,
        ApplicationOpensAt = p.ApplicationOpensAt,
        ApplicationClosesAt = p.ApplicationClosesAt,
        RequiresRepayment = p.RequiresRepayment,
        InterestRatePercent = p.InterestRatePercent,
        RepaymentPeriodMonths = p.RepaymentPeriodMonths,
        ManagedByUserId = p.ManagedByUserId,
        ManagedByName = p.ManagedBy?.FullName,
        ApplicationCount = applicationCount,
        ApprovedCount = approvedCount,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };

    public static FundingProgramListItemDto ToListItemDto(this FundingProgram p, int applicationCount) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Type = p.Type.ToString(),
        Status = p.Status.ToString(),
        Currency = p.Currency,
        TotalBudget = p.TotalBudget,
        AllocatedAmount = p.AllocatedAmount,
        DisbursedAmount = p.DisbursedAmount,
        ApplicationClosesAt = p.ApplicationClosesAt,
        ApplicationCount = applicationCount,
    };

    // ---- Funding applications --------------------------------------

    public static FundingApplicationDto ToDto(this FundingApplication a) => new()
    {
        Id = a.Id,
        FundingProgramId = a.FundingProgramId,
        ProgramName = a.Program?.Name ?? string.Empty,
        ProgramType = a.Program?.Type.ToString() ?? string.Empty,
        ReferenceCode = a.ReferenceCode,
        ApplicantType = a.ApplicantType.ToString(),
        ApplicantUserId = a.ApplicantUserId,
        ApplicantVillageId = a.ApplicantVillageId,
        ApplicantLabel = a.ApplicantLabel,
        Status = a.Status.ToString(),
        RequestedAmount = a.RequestedAmount,
        ApprovedAmount = a.ApprovedAmount,
        Purpose = a.Purpose,
        Justification = a.Justification,
        ContactName = a.ContactName,
        ContactPhone = a.ContactPhone,
        ContactEmail = a.ContactEmail,
        SubmittedAt = a.SubmittedAt,
        DecisionAt = a.DecisionAt,
        DecisionByName = a.DecisionBy?.FullName,
        DecisionNotes = a.DecisionNotes,
        RepaymentStatus = a.RepaymentStatus.ToString(),
        OutstandingBalance = a.OutstandingBalance,
        TotalRepaid = a.TotalRepaid,
        NextRepaymentDueAt = a.NextRepaymentDueAt,
        TotalDisbursed = a.Disbursements
            .Where(d => d.Status == Domain.Entities.Governance.FundingDisbursementStatus.Paid)
            .Sum(d => d.Amount),
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Reviews = a.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FundingApplicationReviewDto
            {
                Id = r.Id,
                ReviewerUserId = r.ReviewerUserId,
                ReviewerName = r.Reviewer?.FullName,
                Decision = r.Decision.ToString(),
                Score = r.Score,
                RecommendedAmount = r.RecommendedAmount,
                Comments = r.Comments,
                CreatedAt = r.CreatedAt,
            })
            .ToList(),
        Disbursements = a.Disbursements
            .OrderBy(d => d.ScheduledFor)
            .Select(d => new FundingDisbursementDto
            {
                Id = d.Id,
                Amount = d.Amount,
                Method = d.Method.ToString(),
                Status = d.Status.ToString(),
                ScheduledFor = d.ScheduledFor,
                PaidAt = d.PaidAt,
                Reference = d.Reference,
                Notes = d.Notes,
                RecordedByName = d.RecordedBy?.FullName,
                CreatedAt = d.CreatedAt,
            })
            .ToList(),
        Events = a.Events
            .OrderBy(e => e.CreatedAt)
            .Select(e => new FundingApplicationEventDto
            {
                Type = e.Type.ToString(),
                Note = e.Note,
                FromStatus = e.FromStatus?.ToString(),
                ToStatus = e.ToStatus?.ToString(),
                ActorUserId = e.ActorUserId,
                ActorName = e.Actor?.FullName,
                CreatedAt = e.CreatedAt,
            })
            .ToList(),
    };

    public static FundingApplicationListItemDto ToListItemDto(this FundingApplication a) => new()
    {
        Id = a.Id,
        FundingProgramId = a.FundingProgramId,
        ProgramName = a.Program?.Name ?? string.Empty,
        ReferenceCode = a.ReferenceCode,
        ApplicantType = a.ApplicantType.ToString(),
        ApplicantLabel = a.ApplicantLabel,
        Status = a.Status.ToString(),
        RequestedAmount = a.RequestedAmount,
        ApprovedAmount = a.ApprovedAmount,
        RepaymentStatus = a.RepaymentStatus.ToString(),
        SubmittedAt = a.SubmittedAt,
    };
}
