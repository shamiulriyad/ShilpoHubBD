using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Assessment;
using PortfolioEntity = ShilpoHubBD.Domain.Entities.Portfolio.Portfolio;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Application.Services.Portfolio;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IAcademyMemberProfileService _profileService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IApprenticeEnrollmentService _apprenticeEnrollmentService;
    private readonly ITrainingCertificateService _trainingCertificateService;
    private readonly IAchievementService _achievementService;
    private readonly IMentorFeedbackService _mentorFeedbackService;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;

    public PortfolioService(
        IPortfolioRepository portfolioRepository,
        IAcademyMemberProfileService profileService,
        IEnrollmentService enrollmentService,
        IApprenticeEnrollmentService apprenticeEnrollmentService,
        ITrainingCertificateService trainingCertificateService,
        IAchievementService achievementService,
        IMentorFeedbackService mentorFeedbackService,
        IAssignmentRepository assignmentRepository,
        IHeritageSkillRepository heritageSkillRepository)
    {
        _portfolioRepository = portfolioRepository;
        _profileService = profileService;
        _enrollmentService = enrollmentService;
        _apprenticeEnrollmentService = apprenticeEnrollmentService;
        _trainingCertificateService = trainingCertificateService;
        _achievementService = achievementService;
        _mentorFeedbackService = mentorFeedbackService;
        _assignmentRepository = assignmentRepository;
        _heritageSkillRepository = heritageSkillRepository;
    }

    public async Task<PortfolioDto> GetMyPortfolioAsync(Guid userId, CancellationToken cancellationToken)
    {
        var (portfolio, profile) = await GetOrCreatePortfolioAsync(userId, cancellationToken);
        return await AssembleAsync(portfolio, profile, cancellationToken);
    }

    public async Task<PortfolioDto> GetPublicPortfolioAsync(Guid academyMemberProfileId, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByAcademyMemberProfileIdAsync(academyMemberProfileId, cancellationToken)
            ?? throw new NotFoundException("Portfolio not found.");

        if (portfolio.Visibility != PortfolioVisibility.Public)
        {
            throw new NotFoundException("Portfolio not found.");
        }

        var profile = await _profileService.GetByIdAsync(academyMemberProfileId, cancellationToken);
        return await AssembleAsync(portfolio, profile, cancellationToken);
    }

    public async Task<PortfolioDto> GetPortfolioForProfileAsync(Guid academyMemberProfileId, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByAcademyMemberProfileIdAsync(academyMemberProfileId, cancellationToken)
            ?? throw new NotFoundException("Portfolio not found.");

        var profile = await _profileService.GetByIdAsync(academyMemberProfileId, cancellationToken);
        return await AssembleAsync(portfolio, profile, cancellationToken);
    }

    public async Task<PortfolioDto> UpdateMyPortfolioAsync(Guid userId, UpdatePortfolioRequest request, CancellationToken cancellationToken)
    {
        var (portfolio, profile) = await GetOrCreatePortfolioAsync(userId, cancellationToken);

        portfolio.Headline = request.Headline.Trim();
        portfolio.Summary = request.Summary.Trim();
        portfolio.UpdatedAt = DateTime.UtcNow;

        await _portfolioRepository.SaveChangesAsync(cancellationToken);
        return await AssembleAsync(portfolio, profile, cancellationToken);
    }

    public async Task<PortfolioDto> UpdateVisibilityAsync(Guid userId, UpdatePortfolioVisibilityRequest request, CancellationToken cancellationToken)
    {
        var (portfolio, profile) = await GetOrCreatePortfolioAsync(userId, cancellationToken);

        if (!Enum.TryParse<PortfolioVisibility>(request.Visibility, true, out var visibility))
        {
            throw new ConflictException("Visibility must be either 'Private' or 'Public'.");
        }

        portfolio.Visibility = visibility;
        portfolio.UpdatedAt = DateTime.UtcNow;

        await _portfolioRepository.SaveChangesAsync(cancellationToken);
        return await AssembleAsync(portfolio, profile, cancellationToken);
    }

    public async Task<PortfolioProjectDto> AddProjectAsync(Guid userId, CreatePortfolioProjectRequest request, CancellationToken cancellationToken)
    {
        var (portfolio, _) = await GetOrCreatePortfolioAsync(userId, cancellationToken);
        await EnsureHeritageSkillExistsAsync(request.HeritageSkillId, cancellationToken);

        var project = new PortfolioProject
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            HeritageSkillId = request.HeritageSkillId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            ProjectUrl = request.ProjectUrl?.Trim(),
            CompletedAt = request.CompletedAt,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
        };

        await _portfolioRepository.AddProjectAsync(project, cancellationToken);
        portfolio.UpdatedAt = DateTime.UtcNow;
        await _portfolioRepository.SaveChangesAsync(cancellationToken);

        var heritageSkill = request.HeritageSkillId.HasValue
            ? await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId.Value, cancellationToken)
            : null;
        return ToProjectDto(project, heritageSkill?.Name);
    }

    public async Task<PortfolioProjectDto> UpdateProjectAsync(
        Guid userId, Guid projectId, UpdatePortfolioProjectRequest request, CancellationToken cancellationToken)
    {
        var (portfolio, _) = await GetOrCreatePortfolioAsync(userId, cancellationToken);
        await EnsureHeritageSkillExistsAsync(request.HeritageSkillId, cancellationToken);

        var project = await GetOwnedProjectAsync(portfolio, projectId, cancellationToken);

        project.HeritageSkillId = request.HeritageSkillId;
        project.Title = request.Title.Trim();
        project.Description = request.Description.Trim();
        project.ImageUrl = request.ImageUrl?.Trim();
        project.ProjectUrl = request.ProjectUrl?.Trim();
        project.CompletedAt = request.CompletedAt;
        project.DisplayOrder = request.DisplayOrder;

        portfolio.UpdatedAt = DateTime.UtcNow;
        await _portfolioRepository.SaveChangesAsync(cancellationToken);

        var heritageSkill = request.HeritageSkillId.HasValue
            ? await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId.Value, cancellationToken)
            : null;
        return ToProjectDto(project, heritageSkill?.Name);
    }

    public async Task DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var (portfolio, _) = await GetOrCreatePortfolioAsync(userId, cancellationToken);
        var project = await GetOwnedProjectAsync(portfolio, projectId, cancellationToken);

        _portfolioRepository.RemoveProject(project);
        portfolio.UpdatedAt = DateTime.UtcNow;
        await _portfolioRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<(PortfolioEntity Portfolio, AcademyMemberProfileDto Profile)> GetOrCreatePortfolioAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetMyProfileAsync(userId, cancellationToken);

        var portfolio = await _portfolioRepository.GetByAcademyMemberProfileIdAsync(profile.Id, cancellationToken);
        if (portfolio is not null)
        {
            return (portfolio, profile);
        }

        var now = DateTime.UtcNow;
        portfolio = new PortfolioEntity
        {
            Id = Guid.NewGuid(),
            AcademyMemberProfileId = profile.Id,
            Headline = string.Empty,
            Summary = string.Empty,
            Visibility = PortfolioVisibility.Private,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _portfolioRepository.AddAsync(portfolio, cancellationToken);
        await _portfolioRepository.SaveChangesAsync(cancellationToken);

        var created = await _portfolioRepository.GetByAcademyMemberProfileIdAsync(profile.Id, cancellationToken);
        return (created!, profile);
    }

    private async Task<PortfolioProject> GetOwnedProjectAsync(PortfolioEntity portfolio, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _portfolioRepository.GetProjectByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        if (project.PortfolioId != portfolio.Id)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this project.");
        }

        return project;
    }

    private async Task EnsureHeritageSkillExistsAsync(Guid? heritageSkillId, CancellationToken cancellationToken)
    {
        if (heritageSkillId.HasValue && await _heritageSkillRepository.GetByIdAsync(heritageSkillId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }
    }

    private async Task<PortfolioDto> AssembleAsync(PortfolioEntity portfolio, AcademyMemberProfileDto profile, CancellationToken cancellationToken)
    {
        var completedCourses = (await _enrollmentService.GetMyEnrollmentsAsync(profile.UserId, cancellationToken))
            .Where(e => e.Status == "Completed")
            .ToList();

        var apprenticeshipExperience = await _apprenticeEnrollmentService.GetMyEnrollmentsAsync(profile.UserId, cancellationToken);
        var certificates = await _trainingCertificateService.GetMineAsync(profile.UserId, cancellationToken);
        var achievements = await _achievementService.GetMyAchievementsAsync(profile.UserId, cancellationToken);
        var mentorFeedback = await _mentorFeedbackService.GetForLearnerAsync(profile.UserId, cancellationToken);

        var submissions = await _assignmentRepository.GetSubmissionsByStudentAsync(profile.UserId, cancellationToken);
        var assignments = submissions
            .Where(s => s.Status == SubmissionStatus.Graded)
            .OrderByDescending(s => s.GradedAt)
            .Select(s => new PortfolioAssignmentDto
            {
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                MaxScore = s.Assignment.MaxScore,
                Score = s.Score,
                Feedback = s.Feedback,
                GradedAt = s.GradedAt,
            })
            .ToList();

        return new PortfolioDto
        {
            Id = portfolio.Id,
            AcademyMemberProfileId = profile.Id,
            MemberName = profile.FullName,
            Headline = portfolio.Headline,
            Summary = portfolio.Summary,
            Visibility = portfolio.Visibility.ToString(),
            HeritageSkills = profile.Skills,
            CompletedCourses = completedCourses,
            Certificates = certificates,
            Projects = portfolio.Projects.OrderBy(p => p.DisplayOrder).Select(p => ToProjectDto(p, p.HeritageSkill?.Name)).ToList(),
            Assignments = assignments,
            Achievements = achievements,
            ApprenticeshipExperience = apprenticeshipExperience,
            MentorFeedback = mentorFeedback,
            CreatedAt = portfolio.CreatedAt,
            UpdatedAt = portfolio.UpdatedAt,
        };
    }

    private static PortfolioProjectDto ToProjectDto(PortfolioProject project, string? heritageSkillName) => new()
    {
        Id = project.Id,
        PortfolioId = project.PortfolioId,
        HeritageSkillId = project.HeritageSkillId,
        HeritageSkillName = heritageSkillName,
        Title = project.Title,
        Description = project.Description,
        ImageUrl = project.ImageUrl,
        ProjectUrl = project.ProjectUrl,
        CompletedAt = project.CompletedAt,
        DisplayOrder = project.DisplayOrder,
        CreatedAt = project.CreatedAt,
    };
}
