using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.BusinessPartner;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Application.Services.Employment;

public class JobListingService : IJobListingService
{
    private readonly IJobListingRepository _jobListingRepository;
    private readonly IBusinessPartnerRepository _businessPartnerRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;

    public JobListingService(
        IJobListingRepository jobListingRepository,
        IBusinessPartnerRepository businessPartnerRepository,
        IHeritageSkillRepository heritageSkillRepository)
    {
        _jobListingRepository = jobListingRepository;
        _businessPartnerRepository = businessPartnerRepository;
        _heritageSkillRepository = heritageSkillRepository;
    }

    public async Task<JobListingDto> CreateAsync(Guid userId, CreateJobListingRequest request, CancellationToken cancellationToken)
    {
        var employer = await ResolveVerifiedEmployerAsync(userId, cancellationToken);

        if (!Enum.TryParse<EmploymentType>(request.EmploymentType, true, out var employmentType))
        {
            throw new ConflictException("EmploymentType must be one of: FullTime, PartTime, Contract, Freelance, Internship.");
        }

        var now = DateTime.UtcNow;
        var jobListing = new JobListing
        {
            Id = Guid.NewGuid(),
            BusinessPartnerProfileId = employer.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location?.Trim(),
            EmploymentType = employmentType,
            MinExperienceYears = request.MinExperienceYears,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Status = JobListingStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _jobListingRepository.AddAsync(jobListing, cancellationToken);
        await _jobListingRepository.SaveChangesAsync(cancellationToken);

        var created = await _jobListingRepository.GetByIdAsync(jobListing.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<JobListingDto> UpdateAsync(Guid userId, Guid jobListingId, UpdateJobListingRequest request, CancellationToken cancellationToken)
    {
        var jobListing = await GetOwnedJobListingAsync(userId, jobListingId, cancellationToken);

        jobListing.Title = request.Title.Trim();
        jobListing.Description = request.Description.Trim();
        jobListing.Location = request.Location?.Trim();
        jobListing.MinExperienceYears = request.MinExperienceYears;
        jobListing.SalaryMin = request.SalaryMin;
        jobListing.SalaryMax = request.SalaryMax;
        jobListing.UpdatedAt = DateTime.UtcNow;

        await _jobListingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(jobListing);
    }

    public async Task<JobListingDto> PublishAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken)
    {
        var jobListing = await GetOwnedJobListingAsync(userId, jobListingId, cancellationToken);

        if (jobListing.Status == JobListingStatus.Published)
        {
            throw new ConflictException("This job listing is already published.");
        }

        if (jobListing.Status != JobListingStatus.Draft)
        {
            throw new ConflictException("Only a draft job listing can be published.");
        }

        var now = DateTime.UtcNow;
        jobListing.Status = JobListingStatus.Published;
        jobListing.PublishedAt = now;
        jobListing.UpdatedAt = now;

        await _jobListingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(jobListing);
    }

    public async Task<JobListingDto> CloseAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken)
    {
        var jobListing = await GetOwnedJobListingAsync(userId, jobListingId, cancellationToken);

        if (jobListing.Status != JobListingStatus.Published)
        {
            throw new ConflictException("Only a published job listing can be closed.");
        }

        jobListing.Status = JobListingStatus.Closed;
        jobListing.UpdatedAt = DateTime.UtcNow;

        await _jobListingRepository.SaveChangesAsync(cancellationToken);
        return ToDto(jobListing);
    }

    public async Task DeleteAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken)
    {
        var jobListing = await GetOwnedJobListingAsync(userId, jobListingId, cancellationToken);

        if (jobListing.Status != JobListingStatus.Draft)
        {
            throw new ConflictException("Only draft job listings can be deleted. Close it instead.");
        }

        _jobListingRepository.Remove(jobListing);
        await _jobListingRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobListingDto> GetByIdAsync(Guid jobListingId, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var jobListing = await _jobListingRepository.GetByIdAsync(jobListingId, cancellationToken)
            ?? throw new NotFoundException("Job listing not found.");

        var isOwner = jobListing.BusinessPartnerProfile.UserId == currentUserId;
        if (jobListing.Status != JobListingStatus.Published && !isOwner)
        {
            throw new NotFoundException("Job listing not found.");
        }

        return ToDto(jobListing);
    }

    public async Task<PagedResult<JobListingListItemDto>> GetPublishedAsync(JobListingQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _jobListingRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<JobListingListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<List<JobListingListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var employer = await _businessPartnerRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ConflictException("You must have a business partner profile before managing job listings.");

        var jobListings = await _jobListingRepository.GetByEmployerAsync(employer.Id, cancellationToken);
        return jobListings.Select(ToListItemDto).ToList();
    }

    public async Task<JobSkillRequirementDto> AddSkillRequirementAsync(
        Guid userId, Guid jobListingId, AddJobSkillRequirementRequest request, CancellationToken cancellationToken)
    {
        var jobListing = await GetOwnedJobListingAsync(userId, jobListingId, cancellationToken);

        var heritageSkill = await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId, cancellationToken)
            ?? throw new NotFoundException("Heritage skill not found.");

        if (jobListing.SkillRequirements.Any(r => r.HeritageSkillId == request.HeritageSkillId))
        {
            throw new ConflictException("This heritage skill is already a requirement on this job listing.");
        }

        var requirement = new JobSkillRequirement
        {
            Id = Guid.NewGuid(),
            JobListingId = jobListing.Id,
            HeritageSkillId = heritageSkill.Id,
            MinLevel = request.MinLevel,
            IsRequired = request.IsRequired,
        };

        await _jobListingRepository.AddSkillRequirementAsync(requirement, cancellationToken);
        jobListing.UpdatedAt = DateTime.UtcNow;
        await _jobListingRepository.SaveChangesAsync(cancellationToken);

        return new JobSkillRequirementDto
        {
            Id = requirement.Id,
            JobListingId = requirement.JobListingId,
            HeritageSkillId = requirement.HeritageSkillId,
            HeritageSkillName = heritageSkill.Name,
            MinLevel = requirement.MinLevel.ToString(),
            IsRequired = requirement.IsRequired,
        };
    }

    public async Task RemoveSkillRequirementAsync(Guid userId, Guid jobListingId, Guid requirementId, CancellationToken cancellationToken)
    {
        await GetOwnedJobListingAsync(userId, jobListingId, cancellationToken);

        var requirement = await _jobListingRepository.GetSkillRequirementByIdAsync(requirementId, cancellationToken)
            ?? throw new NotFoundException("Skill requirement not found.");

        if (requirement.JobListingId != jobListingId)
        {
            throw new NotFoundException("Skill requirement not found.");
        }

        _jobListingRepository.RemoveSkillRequirement(requirement);
        await _jobListingRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<BusinessPartnerProfile> ResolveVerifiedEmployerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var employer = await _businessPartnerRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ConflictException("You must have a business partner profile before posting job listings.");

        if (employer.VerificationStatus != BusinessVerificationStatus.Verified)
        {
            throw new ConflictException("Your business partner profile must be verified before posting job listings.");
        }

        return employer;
    }

    private async Task<JobListing> GetOwnedJobListingAsync(Guid userId, Guid jobListingId, CancellationToken cancellationToken)
    {
        var jobListing = await _jobListingRepository.GetByIdAsync(jobListingId, cancellationToken)
            ?? throw new NotFoundException("Job listing not found.");

        if (jobListing.BusinessPartnerProfile.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this job listing.");
        }

        return jobListing;
    }

    private static JobListingListItemDto ToListItemDto(JobListing jobListing) => new()
    {
        Id = jobListing.Id,
        EmployerName = jobListing.BusinessPartnerProfile.CompanyName,
        Title = jobListing.Title,
        Location = jobListing.Location,
        EmploymentType = jobListing.EmploymentType.ToString(),
        MinExperienceYears = jobListing.MinExperienceYears,
        SalaryMin = jobListing.SalaryMin,
        SalaryMax = jobListing.SalaryMax,
        Status = jobListing.Status.ToString(),
        ApplicationCount = jobListing.Applications.Count,
        PublishedAt = jobListing.PublishedAt,
    };

    private static JobListingDto ToDto(JobListing jobListing) => new()
    {
        Id = jobListing.Id,
        BusinessPartnerProfileId = jobListing.BusinessPartnerProfileId,
        EmployerName = jobListing.BusinessPartnerProfile.CompanyName,
        EmployerIndustry = jobListing.BusinessPartnerProfile.Industry,
        EmployerWebsite = jobListing.BusinessPartnerProfile.Website,
        EmployerCity = jobListing.BusinessPartnerProfile.City,
        Title = jobListing.Title,
        Description = jobListing.Description,
        Location = jobListing.Location,
        EmploymentType = jobListing.EmploymentType.ToString(),
        MinExperienceYears = jobListing.MinExperienceYears,
        SalaryMin = jobListing.SalaryMin,
        SalaryMax = jobListing.SalaryMax,
        Status = jobListing.Status.ToString(),
        SkillRequirements = jobListing.SkillRequirements.Select(r => new JobSkillRequirementDto
        {
            Id = r.Id,
            JobListingId = r.JobListingId,
            HeritageSkillId = r.HeritageSkillId,
            HeritageSkillName = r.HeritageSkill.Name,
            MinLevel = r.MinLevel.ToString(),
            IsRequired = r.IsRequired,
        }).ToList(),
        ApplicationCount = jobListing.Applications.Count,
        CreatedAt = jobListing.CreatedAt,
        UpdatedAt = jobListing.UpdatedAt,
        PublishedAt = jobListing.PublishedAt,
    };
}
