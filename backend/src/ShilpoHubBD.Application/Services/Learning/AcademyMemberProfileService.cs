using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class AcademyMemberProfileService : IAcademyMemberProfileService
{
    private readonly IAcademyMemberProfileRepository _profileRepository;
    private readonly IHeritageSkillRepository _skillRepository;
    private readonly IEnrollmentService _enrollmentService;

    public AcademyMemberProfileService(
        IAcademyMemberProfileRepository profileRepository,
        IHeritageSkillRepository skillRepository,
        IEnrollmentService enrollmentService)
    {
        _profileRepository = profileRepository;
        _skillRepository = skillRepository;
        _enrollmentService = enrollmentService;
    }

    public async Task<AcademyMemberProfileDto> CreateProfileAsync(Guid userId, CreateAcademyMemberProfileRequest request, CancellationToken cancellationToken)
    {
        var existing = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("You already have an academy member profile.");
        }

        var now = DateTime.UtcNow;
        var profile = new AcademyMemberProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = request.Role,
            Bio = request.Bio.Trim(),
            LearningPreferences = request.LearningPreferences.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _profileRepository.AddAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        var created = await _profileRepository.GetByIdAsync(profile.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<AcademyMemberProfileDto> UpdateProfileAsync(Guid userId, UpdateAcademyMemberProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        profile.Role = request.Role;
        profile.Bio = request.Bio.Trim();
        profile.LearningPreferences = request.LearningPreferences.Trim();
        profile.UpdatedAt = DateTime.UtcNow;

        await _profileRepository.SaveChangesAsync(cancellationToken);
        return ToDto(profile);
    }

    public async Task<AcademyMemberProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        return ToDto(profile);
    }

    public async Task<AcademyMemberProfileDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        return ToDto(profile);
    }

    public async Task<AcademyMemberProfileDto> AddSkillAsync(Guid userId, AddMemberSkillRequest request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        if (await _skillRepository.GetByIdAsync(request.HeritageSkillId, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }

        var existingSkill = await _profileRepository.GetSkillAsync(profile.Id, request.HeritageSkillId, cancellationToken);
        if (existingSkill is not null)
        {
            existingSkill.Level = request.Level;
            await _profileRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await _profileRepository.AddSkillAsync(new AcademyMemberSkill
            {
                Id = Guid.NewGuid(),
                AcademyMemberProfileId = profile.Id,
                HeritageSkillId = request.HeritageSkillId,
                Level = request.Level,
                AddedAt = DateTime.UtcNow,
            }, cancellationToken);
            await _profileRepository.SaveChangesAsync(cancellationToken);
        }

        var updated = await _profileRepository.GetByIdAsync(profile.Id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task<AcademyMemberProfileDto> RemoveSkillAsync(Guid userId, Guid heritageSkillId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        var skill = await _profileRepository.GetSkillAsync(profile.Id, heritageSkillId, cancellationToken)
            ?? throw new NotFoundException("This skill is not on your profile.");

        _profileRepository.RemoveSkill(skill);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        var updated = await _profileRepository.GetByIdAsync(profile.Id, cancellationToken);
        return ToDto(updated!);
    }

    public Task<List<EnrollmentListItemDto>> GetMyLearningHistoryAsync(Guid userId, CancellationToken cancellationToken)
        => _enrollmentService.GetMyEnrollmentsAsync(userId, cancellationToken);

    private static AcademyMemberProfileDto ToDto(AcademyMemberProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        FullName = profile.User.FullName,
        Role = profile.Role.ToString(),
        Bio = profile.Bio,
        LearningPreferences = profile.LearningPreferences,
        Skills = profile.Skills.Select(s => new AcademyMemberSkillDto
        {
            Id = s.Id,
            HeritageSkillId = s.HeritageSkillId,
            HeritageSkillName = s.HeritageSkill.Name,
            Level = s.Level.ToString(),
            AddedAt = s.AddedAt,
        }).ToList(),
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt,
    };
}
