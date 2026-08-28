using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class MentorService : IMentorService
{
    private readonly IMentorRepository _mentorRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;

    public MentorService(IMentorRepository mentorRepository, IHeritageSkillRepository heritageSkillRepository)
    {
        _mentorRepository = mentorRepository;
        _heritageSkillRepository = heritageSkillRepository;
    }

    public async Task<MentorProfileDto> BecomeMentorAsync(Guid userId, BecomeMentorRequest request, CancellationToken cancellationToken)
    {
        var existing = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("You already have a mentor profile.");
        }

        var now = DateTime.UtcNow;
        var mentor = new MentorProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Bio = request.Bio.Trim(),
            Expertise = request.Expertise.Trim(),
            YearsOfExperience = request.YearsOfExperience,
            IsActive = true,
            Location = request.Location?.Trim(),
            AvailabilityNote = request.AvailabilityNote?.Trim(),
            PreferredCategory = request.PreferredCategory?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _mentorRepository.AddAsync(mentor, cancellationToken);
        await _mentorRepository.SaveChangesAsync(cancellationToken);

        var created = await _mentorRepository.GetByIdAsync(mentor.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<MentorProfileDto> UpdateProfileAsync(Guid userId, UpdateMentorProfileRequest request, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        mentor.Bio = request.Bio.Trim();
        mentor.Expertise = request.Expertise.Trim();
        mentor.YearsOfExperience = request.YearsOfExperience;
        mentor.IsActive = request.IsActive;
        mentor.Location = request.Location?.Trim();
        mentor.AvailabilityNote = request.AvailabilityNote?.Trim();
        mentor.PreferredCategory = request.PreferredCategory?.Trim();
        mentor.UpdatedAt = DateTime.UtcNow;

        await _mentorRepository.SaveChangesAsync(cancellationToken);
        return ToDto(mentor);
    }

    public async Task<MentorProfileDto> AddSkillAsync(Guid userId, AddMentorSkillRequest request, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        if (await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }

        var existingSkill = await _mentorRepository.GetSkillAsync(mentor.Id, request.HeritageSkillId, cancellationToken);
        if (existingSkill is not null)
        {
            existingSkill.Level = request.Level;
        }
        else
        {
            await _mentorRepository.AddSkillAsync(new MentorSkill
            {
                Id = Guid.NewGuid(),
                MentorProfileId = mentor.Id,
                HeritageSkillId = request.HeritageSkillId,
                Level = request.Level,
                AddedAt = DateTime.UtcNow,
            }, cancellationToken);
        }

        await _mentorRepository.SaveChangesAsync(cancellationToken);

        var updated = await _mentorRepository.GetByIdAsync(mentor.Id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task<MentorProfileDto> RemoveSkillAsync(Guid userId, Guid heritageSkillId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        var skill = await _mentorRepository.GetSkillAsync(mentor.Id, heritageSkillId, cancellationToken)
            ?? throw new NotFoundException("This skill is not on your mentor profile.");

        _mentorRepository.RemoveSkill(skill);
        await _mentorRepository.SaveChangesAsync(cancellationToken);

        var updated = await _mentorRepository.GetByIdAsync(mentor.Id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task<MentorProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        return ToDto(mentor);
    }

    public async Task<MentorProfileDto> GetByIdAsync(Guid mentorId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByIdAsync(mentorId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        return ToDto(mentor);
    }

    public async Task<PagedResult<MentorListItemDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _mentorRepository.GetPagedAsync(true, page, pageSize, cancellationToken);
        return new PagedResult<MentorListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    private static MentorListItemDto ToListItemDto(MentorProfile mentor) => new()
    {
        Id = mentor.Id,
        UserId = mentor.UserId,
        FullName = mentor.User.FullName,
        Expertise = mentor.Expertise,
        YearsOfExperience = mentor.YearsOfExperience,
        PublishedCourseCount = mentor.Courses.Count(c => c.Status == CourseStatus.Published),
    };

    private static MentorProfileDto ToDto(MentorProfile mentor) => new()
    {
        Id = mentor.Id,
        UserId = mentor.UserId,
        FullName = mentor.User.FullName,
        Bio = mentor.Bio,
        Expertise = mentor.Expertise,
        YearsOfExperience = mentor.YearsOfExperience,
        IsActive = mentor.IsActive,
        Location = mentor.Location,
        AvailabilityNote = mentor.AvailabilityNote,
        PreferredCategory = mentor.PreferredCategory,
        Skills = mentor.Skills.Select(s => new MentorSkillDto
        {
            Id = s.Id,
            HeritageSkillId = s.HeritageSkillId,
            HeritageSkillName = s.HeritageSkill.Name,
            Level = s.Level.ToString(),
            AddedAt = s.AddedAt,
        }).ToList(),
        PublishedCourseCount = mentor.Courses.Count(c => c.Status == CourseStatus.Published),
        CreatedAt = mentor.CreatedAt,
        UpdatedAt = mentor.UpdatedAt,
    };
}
