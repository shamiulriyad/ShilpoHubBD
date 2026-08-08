using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.HeritageIdentity;

public class HeritageIdentityService : IHeritageIdentityService
{
    private readonly IHeritageIdentityRepository _heritageIdentityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMentorRepository _mentorRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly HeritageScoreOptions _scoreOptions;

    public HeritageIdentityService(
        IHeritageIdentityRepository heritageIdentityRepository,
        IUserRepository userRepository,
        IDistrictRepository districtRepository,
        IProductRepository productRepository,
        IMentorRepository mentorRepository,
        IEnrollmentRepository enrollmentRepository,
        IOptions<HeritageScoreOptions> scoreOptions)
    {
        _heritageIdentityRepository = heritageIdentityRepository;
        _userRepository = userRepository;
        _districtRepository = districtRepository;
        _productRepository = productRepository;
        _mentorRepository = mentorRepository;
        _enrollmentRepository = enrollmentRepository;
        _scoreOptions = scoreOptions.Value;
    }

    public async Task<ProducerHeritageIdentityDto> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Heritage identity not found for this producer.");

        return ToDto(identity);
    }

    public async Task<PagedResult<ProducerHeritageIdentityDto>> GetVerifiedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _heritageIdentityRepository.GetPagedVerifiedAsync(page, pageSize, cancellationToken);

        return new PagedResult<ProducerHeritageIdentityDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ProducerHeritageIdentityDto> UpsertAsync(
        Guid producerId, Guid currentUserId, bool isAdmin, UpsertHeritageIdentityRequest request, CancellationToken cancellationToken)
    {
        if (!isAdmin && producerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this producer's heritage identity.");
        }

        var producer = await _userRepository.GetByIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        if (request.DistrictId.HasValue
            && await _districtRepository.GetByIdAsync(request.DistrictId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken);
        var now = DateTime.UtcNow;

        if (identity is null)
        {
            identity = new ProducerHeritageIdentity
            {
                Id = Guid.NewGuid(),
                ProducerId = producerId,
                HeritageIdNumber = await GenerateUniqueHeritageIdNumberAsync(cancellationToken),
                VerificationStatus = HeritageVerificationStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _heritageIdentityRepository.AddAsync(identity, cancellationToken);
        }

        identity.PrimaryCraft = request.PrimaryCraft.Trim();
        identity.YearsOfExperience = request.YearsOfExperience;
        identity.WorkshopName = request.WorkshopName.Trim();
        identity.WorkshopDescription = request.WorkshopDescription.Trim();
        identity.WorkshopAddress = string.IsNullOrWhiteSpace(request.WorkshopAddress) ? null : request.WorkshopAddress.Trim();
        identity.EstablishedYear = request.EstablishedYear;
        identity.DistrictId = request.DistrictId;
        identity.UpdatedAt = now;

        identity.FamilyMembers.Clear();
        AttachFamilyMembers(identity, request.FamilyMembers);

        identity.SkillTimeline.Clear();
        AttachSkillTimeline(identity, request.SkillTimeline);

        identity.Awards.Clear();
        AttachAwards(identity, request.Awards);

        identity.Certifications.Clear();
        AttachCertifications(identity, request.Certifications);

        identity.StoryArchive.Clear();
        AttachStoryArchive(identity, request.StoryArchive);

        await ApplyScoreAsync(identity, cancellationToken);

        await _heritageIdentityRepository.SaveChangesAsync(cancellationToken);

        identity.Producer = producer;
        return ToDto(identity);
    }

    public async Task<ProducerHeritageIdentityDto> VerifyAsync(Guid producerId, Guid verifierUserId, VerifyHeritageIdentityRequest request, CancellationToken cancellationToken)
    {
        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Heritage identity not found for this producer.");

        var verifier = await _userRepository.GetByIdAsync(verifierUserId, cancellationToken)
            ?? throw new NotFoundException("Verifying user not found.");

        identity.VerificationStatus = request.Status;
        identity.VerifiedByUserId = verifierUserId;
        identity.VerificationNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        identity.VerifiedAt = DateTime.UtcNow;
        identity.UpdatedAt = DateTime.UtcNow;

        await ApplyScoreAsync(identity, cancellationToken);

        await _heritageIdentityRepository.SaveChangesAsync(cancellationToken);

        identity.VerifiedBy = verifier;
        return ToDto(identity);
    }

    public async Task DeleteAsync(Guid producerId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Heritage identity not found for this producer.");

        if (!isAdmin && identity.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this producer's heritage identity.");
        }

        _heritageIdentityRepository.Remove(identity);
        await _heritageIdentityRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<LegacyScoreDto> GetScoreAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Heritage identity not found for this producer.");

        // Serve the last persisted score+breakdown pair (they were computed together) rather than
        // pairing the cached score with a freshly recomputed breakdown, which could disagree if
        // product/review/course data changed since the last recalculation.
        var latestHistory = identity.ScoreHistory.OrderByDescending(h => h.CalculatedAt).FirstOrDefault();
        if (latestHistory is not null)
        {
            return new LegacyScoreDto
            {
                ProducerId = producerId,
                Score = latestHistory.Score,
                Breakdown = ToBreakdownDto(latestHistory),
                CalculatedAt = latestHistory.CalculatedAt,
            };
        }

        // No history yet (score has never been calculated) -- compute live so the response is at
        // least internally consistent, without persisting a history row.
        var breakdown = await ComputeBreakdownAsync(identity, cancellationToken);
        return new LegacyScoreDto
        {
            ProducerId = producerId,
            Score = identity.LegacyScore,
            Breakdown = breakdown,
            CalculatedAt = null,
        };
    }

    public async Task<PagedResult<LegacyScoreHistoryEntryDto>> GetScoreHistoryAsync(
        Guid producerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Heritage identity not found for this producer.");

        var (items, totalCount) = await _heritageIdentityRepository.GetScoreHistoryAsync(identity.Id, page, pageSize, cancellationToken);

        return new PagedResult<LegacyScoreHistoryEntryDto>
        {
            Items = items.Select(ToHistoryDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<LegacyScoreDto> RecalculateScoreAsync(
        Guid producerId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (!isAdmin && producerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to recalculate this producer's score.");
        }

        var identity = await _heritageIdentityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Heritage identity not found for this producer.");

        var breakdown = await ApplyScoreAsync(identity, cancellationToken);
        await _heritageIdentityRepository.SaveChangesAsync(cancellationToken);

        return new LegacyScoreDto
        {
            ProducerId = producerId,
            Score = identity.LegacyScore,
            Breakdown = breakdown,
            CalculatedAt = DateTime.UtcNow,
        };
    }

    // Simple, explainable weighting -- not AI-driven. Rewards documented lineage, verified status,
    // marketplace traction (products/reviews) and mentorship contribution (apprentices/courses).
    // Weights are configurable via HeritageScoreOptions. Reuses Product/Course/Enrollment data
    // rather than duplicating it.
    private async Task<LegacyScoreBreakdownDto> ComputeBreakdownAsync(ProducerHeritageIdentity identity, CancellationToken cancellationToken)
    {
        var cappedYears = Math.Min(identity.YearsOfExperience, _scoreOptions.MaxYearsOfExperienceForScoring);

        var products = await _productRepository.GetByProducerAsync(identity.ProducerId, cancellationToken);
        var cappedProductCount = Math.Min(products.Count, _scoreOptions.MaxProductsForScoring);
        var cappedReviewCount = Math.Min(products.Sum(p => p.ReviewCount), _scoreOptions.MaxReviewsForScoring);

        var mentor = await _mentorRepository.GetByUserIdAsync(identity.ProducerId, cancellationToken);
        var publishedCourseCount = mentor?.Courses.Count(c => c.Status == CourseStatus.Published) ?? 0;
        var apprenticesTrainedCount = mentor is null
            ? 0
            : await _enrollmentRepository.GetCompletedCountByMentorAsync(mentor.Id, cancellationToken);

        return new LegacyScoreBreakdownDto
        {
            YearsOfExperiencePoints = cappedYears * _scoreOptions.PointsPerYearOfExperience,
            VerificationPoints = identity.VerificationStatus == HeritageVerificationStatus.Verified ? _scoreOptions.VerifiedBonusPoints : 0,
            AwardsPoints = identity.Awards.Count * _scoreOptions.PointsPerAward,
            CertificationsPoints = identity.Certifications.Count * _scoreOptions.PointsPerCertification,
            ProductsPoints = cappedProductCount * _scoreOptions.PointsPerProduct,
            ReviewsPoints = cappedReviewCount * _scoreOptions.PointsPerReviewReceived,
            ApprenticesTrainedPoints = apprenticesTrainedCount * _scoreOptions.PointsPerApprenticeTrained,
            CoursesPublishedPoints = publishedCourseCount * _scoreOptions.PointsPerPublishedCourse,
            CulturalContributionPoints =
                (identity.FamilyMembers.Count * _scoreOptions.PointsPerFamilyMember) +
                (identity.SkillTimeline.Count * _scoreOptions.PointsPerSkillTimelineEntry) +
                (identity.StoryArchive.Count * _scoreOptions.PointsPerStoryArchiveEntry),
        };
    }

    private async Task<LegacyScoreBreakdownDto> ApplyScoreAsync(ProducerHeritageIdentity identity, CancellationToken cancellationToken)
    {
        var breakdown = await ComputeBreakdownAsync(identity, cancellationToken);
        var totalScore =
            breakdown.YearsOfExperiencePoints +
            breakdown.VerificationPoints +
            breakdown.AwardsPoints +
            breakdown.CertificationsPoints +
            breakdown.ProductsPoints +
            breakdown.ReviewsPoints +
            breakdown.ApprenticesTrainedPoints +
            breakdown.CoursesPublishedPoints +
            breakdown.CulturalContributionPoints;

        var now = DateTime.UtcNow;
        identity.LegacyScore = totalScore;

        await _heritageIdentityRepository.AddScoreHistoryEntryAsync(new ScoreHistoryEntry
        {
            Id = Guid.NewGuid(),
            ProducerHeritageIdentityId = identity.Id,
            Score = totalScore,
            YearsOfExperiencePoints = breakdown.YearsOfExperiencePoints,
            VerificationPoints = breakdown.VerificationPoints,
            AwardsPoints = breakdown.AwardsPoints,
            CertificationsPoints = breakdown.CertificationsPoints,
            ProductsPoints = breakdown.ProductsPoints,
            ReviewsPoints = breakdown.ReviewsPoints,
            ApprenticesTrainedPoints = breakdown.ApprenticesTrainedPoints,
            CoursesPublishedPoints = breakdown.CoursesPublishedPoints,
            CulturalContributionPoints = breakdown.CulturalContributionPoints,
            CalculatedAt = now,
        }, cancellationToken);

        return breakdown;
    }

    private static LegacyScoreHistoryEntryDto ToHistoryDto(ScoreHistoryEntry entry) => new()
    {
        Id = entry.Id,
        Score = entry.Score,
        Breakdown = ToBreakdownDto(entry),
        CalculatedAt = entry.CalculatedAt,
    };

    private static LegacyScoreBreakdownDto ToBreakdownDto(ScoreHistoryEntry entry) => new()
    {
        YearsOfExperiencePoints = entry.YearsOfExperiencePoints,
        VerificationPoints = entry.VerificationPoints,
        AwardsPoints = entry.AwardsPoints,
        CertificationsPoints = entry.CertificationsPoints,
        ProductsPoints = entry.ProductsPoints,
        ReviewsPoints = entry.ReviewsPoints,
        ApprenticesTrainedPoints = entry.ApprenticesTrainedPoints,
        CoursesPublishedPoints = entry.CoursesPublishedPoints,
        CulturalContributionPoints = entry.CulturalContributionPoints,
    };

    private async Task<string> GenerateUniqueHeritageIdNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string heritageIdNumber;

        do
        {
            heritageIdNumber = $"SHB-HID-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _heritageIdentityRepository.ExistsByHeritageIdNumberAsync(heritageIdNumber, cancellationToken));

        return heritageIdNumber;
    }

    private static void AttachFamilyMembers(ProducerHeritageIdentity identity, List<FamilyHeritageMemberInput> members)
    {
        for (var i = 0; i < members.Count; i++)
        {
            identity.FamilyMembers.Add(new FamilyHeritageMember
            {
                Id = Guid.NewGuid(),
                FullName = members[i].FullName.Trim(),
                Relation = members[i].Relation.Trim(),
                Generation = members[i].Generation,
                Role = string.IsNullOrWhiteSpace(members[i].Role) ? null : members[i].Role!.Trim(),
                ActiveYearsRange = string.IsNullOrWhiteSpace(members[i].ActiveYearsRange) ? null : members[i].ActiveYearsRange!.Trim(),
                Story = string.IsNullOrWhiteSpace(members[i].Story) ? null : members[i].Story!.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static void AttachSkillTimeline(ProducerHeritageIdentity identity, List<SkillTimelineEntryInput> entries)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            identity.SkillTimeline.Add(new SkillTimelineEntry
            {
                Id = Guid.NewGuid(),
                Title = entries[i].Title.Trim(),
                Description = entries[i].Description.Trim(),
                Year = entries[i].Year,
                DisplayOrder = i,
            });
        }
    }

    private static void AttachAwards(ProducerHeritageIdentity identity, List<HeritageAwardInput> awards)
    {
        for (var i = 0; i < awards.Count; i++)
        {
            identity.Awards.Add(new HeritageAward
            {
                Id = Guid.NewGuid(),
                Title = awards[i].Title.Trim(),
                IssuingOrganization = awards[i].IssuingOrganization.Trim(),
                Year = awards[i].Year,
                Description = string.IsNullOrWhiteSpace(awards[i].Description) ? null : awards[i].Description!.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(awards[i].ImageUrl) ? null : awards[i].ImageUrl!.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static void AttachCertifications(ProducerHeritageIdentity identity, List<HeritageCertificationInput> certifications)
    {
        for (var i = 0; i < certifications.Count; i++)
        {
            identity.Certifications.Add(new HeritageCertification
            {
                Id = Guid.NewGuid(),
                Name = certifications[i].Name.Trim(),
                IssuingBody = certifications[i].IssuingBody.Trim(),
                IssuedYear = certifications[i].IssuedYear,
                ExpiryYear = certifications[i].ExpiryYear,
                CertificateNumber = string.IsNullOrWhiteSpace(certifications[i].CertificateNumber) ? null : certifications[i].CertificateNumber!.Trim(),
                CertificateUrl = string.IsNullOrWhiteSpace(certifications[i].CertificateUrl) ? null : certifications[i].CertificateUrl!.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static void AttachStoryArchive(ProducerHeritageIdentity identity, List<StoryArchiveEntryInput> entries)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            identity.StoryArchive.Add(new StoryArchiveEntry
            {
                Id = Guid.NewGuid(),
                Title = entries[i].Title.Trim(),
                Content = entries[i].Content.Trim(),
                Year = entries[i].Year,
                DisplayOrder = i,
            });
        }
    }

    private static ProducerHeritageIdentityDto ToDto(ProducerHeritageIdentity identity) => new()
    {
        Id = identity.Id,
        ProducerId = identity.ProducerId,
        ProducerName = identity.Producer.FullName,
        HeritageIdNumber = identity.HeritageIdNumber,
        PrimaryCraft = identity.PrimaryCraft,
        YearsOfExperience = identity.YearsOfExperience,
        WorkshopName = identity.WorkshopName,
        WorkshopDescription = identity.WorkshopDescription,
        WorkshopAddress = identity.WorkshopAddress,
        EstablishedYear = identity.EstablishedYear,
        DistrictId = identity.DistrictId,
        DistrictName = identity.District?.Name,
        VerificationStatus = identity.VerificationStatus,
        VerifiedByName = identity.VerifiedBy?.FullName,
        VerificationNotes = identity.VerificationNotes,
        VerifiedAt = identity.VerifiedAt,
        LegacyScore = identity.LegacyScore,
        FamilyMembers = identity.FamilyMembers
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new FamilyHeritageMemberDto
            {
                FullName = m.FullName,
                Relation = m.Relation,
                Generation = m.Generation,
                Role = m.Role,
                ActiveYearsRange = m.ActiveYearsRange,
                Story = m.Story,
                DisplayOrder = m.DisplayOrder,
            })
            .ToList(),
        SkillTimeline = identity.SkillTimeline
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SkillTimelineEntryDto
            {
                Title = s.Title,
                Description = s.Description,
                Year = s.Year,
                DisplayOrder = s.DisplayOrder,
            })
            .ToList(),
        Awards = identity.Awards
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new HeritageAwardDto
            {
                Title = a.Title,
                IssuingOrganization = a.IssuingOrganization,
                Year = a.Year,
                Description = a.Description,
                ImageUrl = a.ImageUrl,
                DisplayOrder = a.DisplayOrder,
            })
            .ToList(),
        Certifications = identity.Certifications
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new HeritageCertificationDto
            {
                Name = c.Name,
                IssuingBody = c.IssuingBody,
                IssuedYear = c.IssuedYear,
                ExpiryYear = c.ExpiryYear,
                CertificateNumber = c.CertificateNumber,
                CertificateUrl = c.CertificateUrl,
                DisplayOrder = c.DisplayOrder,
            })
            .ToList(),
        StoryArchive = identity.StoryArchive
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new StoryArchiveEntryDto
            {
                Title = s.Title,
                Content = s.Content,
                Year = s.Year,
                DisplayOrder = s.DisplayOrder,
            })
            .ToList(),
        CreatedAt = identity.CreatedAt,
        UpdatedAt = identity.UpdatedAt,
    };
}
