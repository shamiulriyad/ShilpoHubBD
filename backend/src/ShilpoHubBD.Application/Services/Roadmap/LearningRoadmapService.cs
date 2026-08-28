using ShilpoHubBD.Application.DTOs.Roadmap;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Roadmap;

namespace ShilpoHubBD.Application.Services.Roadmap;

public class LearningRoadmapService : ILearningRoadmapService
{
    private const int CourseCandidateTake = 3;
    private const int LessonCandidateTakePerCourse = 3;

    private readonly ILearningRoadmapRepository _roadmapRepository;
    private readonly IAcademyMemberProfileRepository _profileRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;
    private readonly ILearningRoadmapProvider _provider;

    public LearningRoadmapService(
        ILearningRoadmapRepository roadmapRepository,
        IAcademyMemberProfileRepository profileRepository,
        IHeritageSkillRepository heritageSkillRepository,
        ILearningRoadmapProvider provider)
    {
        _roadmapRepository = roadmapRepository;
        _profileRepository = profileRepository;
        _heritageSkillRepository = heritageSkillRepository;
        _provider = provider;
    }

    public async Task<LearningRoadmapDto> CreateAsync(Guid userId, CreateRoadmapRequest request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ConflictException("You must have an academy member profile before generating a learning roadmap.");

        if (request.TargetHeritageSkillId.HasValue
            && await _heritageSkillRepository.GetByIdAsync(request.TargetHeritageSkillId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }

        var existingActive = await _roadmapRepository.GetActiveByProfileAsync(profile.Id, cancellationToken);
        var now = DateTime.UtcNow;
        if (existingActive is not null)
        {
            existingActive.Status = RoadmapStatus.Archived;
            existingActive.UpdatedAt = now;
        }

        var input = await BuildInputAsync(request, profile, cancellationToken);
        var result = await _provider.GenerateAsync(input, cancellationToken);

        var roadmap = new LearningRoadmap
        {
            Id = Guid.NewGuid(),
            AcademyMemberProfileId = profile.Id,
            Goal = request.Goal.Trim(),
            TargetHeritageSkillId = request.TargetHeritageSkillId,
            Status = RoadmapStatus.Active,
            GeneratedAt = now,
            UpdatedAt = now,
        };

        var milestoneOrder = 0;
        foreach (var milestonePlan in result.Milestones)
        {
            var milestone = new RoadmapMilestone
            {
                Id = Guid.NewGuid(),
                HeritageSkillId = milestonePlan.HeritageSkillId,
                TargetLevel = milestonePlan.TargetLevel,
                DisplayOrder = milestoneOrder++,
                IsCompleted = milestonePlan.IsAlreadyCompleted,
                CompletedAt = milestonePlan.IsAlreadyCompleted ? now : null,
            };

            var courseOrder = 0;
            foreach (var coursePlan in milestonePlan.RecommendedCourses)
            {
                milestone.RecommendedCourses.Add(new RoadmapRecommendedCourse
                {
                    Id = Guid.NewGuid(),
                    CourseId = coursePlan.CourseId,
                    Reason = coursePlan.Reason,
                    DisplayOrder = courseOrder++,
                });
            }

            var lessonOrder = 0;
            foreach (var lessonPlan in milestonePlan.RecommendedLessons)
            {
                milestone.RecommendedLessons.Add(new RoadmapRecommendedLesson
                {
                    Id = Guid.NewGuid(),
                    CourseLessonId = lessonPlan.CourseLessonId,
                    Reason = lessonPlan.Reason,
                    DisplayOrder = lessonOrder++,
                });
            }

            roadmap.Milestones.Add(milestone);
        }

        if (roadmap.Milestones.Count > 0 && roadmap.Milestones.All(m => m.IsCompleted))
        {
            roadmap.Status = RoadmapStatus.Completed;
            roadmap.CompletedAt = now;
        }

        await _roadmapRepository.AddAsync(roadmap, cancellationToken);
        await _roadmapRepository.SaveChangesAsync(cancellationToken);

        var created = await _roadmapRepository.GetByIdAsync(roadmap.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<LearningRoadmapDto> GetActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        var roadmap = await _roadmapRepository.GetActiveByProfileAsync(profile.Id, cancellationToken)
            ?? throw new NotFoundException("No active learning roadmap found.");

        return ToDto(roadmap);
    }

    public async Task<LearningRoadmapDto> GetByIdAsync(Guid userId, Guid roadmapId, CancellationToken cancellationToken)
    {
        var roadmap = await GetOwnedRoadmapAsync(userId, roadmapId, cancellationToken);
        return ToDto(roadmap);
    }

    public async Task<List<LearningRoadmapListItemDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        var roadmaps = await _roadmapRepository.GetByProfileAsync(profile.Id, cancellationToken);
        return roadmaps.Select(ToListItemDto).ToList();
    }

    public async Task<LearningRoadmapDto> RefreshProgressAsync(Guid userId, Guid roadmapId, CancellationToken cancellationToken)
    {
        var roadmap = await GetOwnedRoadmapAsync(userId, roadmapId, cancellationToken);

        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Academy member profile not found.");

        var now = DateTime.UtcNow;
        foreach (var milestone in roadmap.Milestones.Where(m => !m.IsCompleted))
        {
            var currentLevel = profile.Skills.FirstOrDefault(s => s.HeritageSkillId == milestone.HeritageSkillId)?.Level;
            if (currentLevel.HasValue && currentLevel.Value >= milestone.TargetLevel)
            {
                milestone.IsCompleted = true;
                milestone.CompletedAt = now;
            }
        }

        if (roadmap.Status == RoadmapStatus.Active && roadmap.Milestones.Count > 0 && roadmap.Milestones.All(m => m.IsCompleted))
        {
            roadmap.Status = RoadmapStatus.Completed;
            roadmap.CompletedAt = now;
        }

        roadmap.UpdatedAt = now;
        await _roadmapRepository.SaveChangesAsync(cancellationToken);
        return ToDto(roadmap);
    }

    public async Task<LearningRoadmapDto> CompleteMilestoneAsync(Guid userId, Guid roadmapId, Guid milestoneId, CancellationToken cancellationToken)
    {
        var roadmap = await GetOwnedRoadmapAsync(userId, roadmapId, cancellationToken);

        var milestone = roadmap.Milestones.FirstOrDefault(m => m.Id == milestoneId)
            ?? throw new NotFoundException("Milestone not found.");

        if (!milestone.IsCompleted)
        {
            var now = DateTime.UtcNow;
            milestone.IsCompleted = true;
            milestone.CompletedAt = now;
            roadmap.UpdatedAt = now;

            if (roadmap.Status == RoadmapStatus.Active && roadmap.Milestones.All(m => m.IsCompleted))
            {
                roadmap.Status = RoadmapStatus.Completed;
                roadmap.CompletedAt = now;
            }

            await _roadmapRepository.SaveChangesAsync(cancellationToken);
        }

        return ToDto(roadmap);
    }

    private async Task<RoadmapGenerationInput> BuildInputAsync(
        CreateRoadmapRequest request, Domain.Entities.Learning.AcademyMemberProfile profile, CancellationToken cancellationToken)
    {
        var currentSkills = profile.Skills
            .Select(s => new SkillProgressInput { HeritageSkillId = s.HeritageSkillId, Name = s.HeritageSkill.Name, CurrentLevel = s.Level })
            .ToList();

        var allSkills = await _heritageSkillRepository.GetAllAsync(true, cancellationToken);

        var candidateSkills = new List<SkillPlanningInput>();
        foreach (var skill in allSkills)
        {
            var candidateCourses = await _roadmapRepository.FindCandidateCoursesAsync(
                skill.Name, CourseCandidateTake, LessonCandidateTakePerCourse, cancellationToken);

            candidateSkills.Add(new SkillPlanningInput
            {
                HeritageSkillId = skill.Id,
                Name = skill.Name,
                CurrentLevel = currentSkills.FirstOrDefault(s => s.HeritageSkillId == skill.Id)?.CurrentLevel,
                Courses = candidateCourses.Select(c => new CandidateCourseInput
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Lessons = c.Lessons.Select(l => new CandidateLessonInput { CourseLessonId = l.Id, Title = l.Title }).ToList(),
                }).ToList(),
            });
        }

        return new RoadmapGenerationInput
        {
            Goal = request.Goal.Trim(),
            TargetHeritageSkillId = request.TargetHeritageSkillId,
            CurrentSkills = currentSkills,
            CandidateSkills = candidateSkills,
        };
    }

    private async Task<LearningRoadmap> GetOwnedRoadmapAsync(Guid userId, Guid roadmapId, CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapRepository.GetByIdAsync(roadmapId, cancellationToken)
            ?? throw new NotFoundException("Learning roadmap not found.");

        if (roadmap.AcademyMemberProfile.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this roadmap.");
        }

        return roadmap;
    }

    private static LearningRoadmapListItemDto ToListItemDto(LearningRoadmap roadmap)
    {
        var total = roadmap.Milestones.Count;
        var completed = roadmap.Milestones.Count(m => m.IsCompleted);

        return new LearningRoadmapListItemDto
        {
            Id = roadmap.Id,
            Goal = roadmap.Goal,
            Status = roadmap.Status.ToString(),
            CompletedMilestoneCount = completed,
            TotalMilestoneCount = total,
            ProgressPercent = total == 0 ? 0 : Math.Round(completed / (decimal)total * 100m, 1),
            GeneratedAt = roadmap.GeneratedAt,
        };
    }

    private static LearningRoadmapDto ToDto(LearningRoadmap roadmap)
    {
        var orderedMilestones = roadmap.Milestones.OrderBy(m => m.DisplayOrder).Select(ToMilestoneDto).ToList();
        var total = orderedMilestones.Count;
        var completed = orderedMilestones.Count(m => m.IsCompleted);

        return new LearningRoadmapDto
        {
            Id = roadmap.Id,
            Goal = roadmap.Goal,
            TargetHeritageSkillId = roadmap.TargetHeritageSkillId,
            TargetHeritageSkillName = roadmap.TargetHeritageSkill?.Name,
            Status = roadmap.Status.ToString(),
            Milestones = orderedMilestones,
            NextStep = orderedMilestones.FirstOrDefault(m => !m.IsCompleted),
            CompletedMilestoneCount = completed,
            TotalMilestoneCount = total,
            ProgressPercent = total == 0 ? 0 : Math.Round(completed / (decimal)total * 100m, 1),
            GeneratedAt = roadmap.GeneratedAt,
            UpdatedAt = roadmap.UpdatedAt,
            CompletedAt = roadmap.CompletedAt,
        };
    }

    private static RoadmapMilestoneDto ToMilestoneDto(RoadmapMilestone milestone) => new()
    {
        Id = milestone.Id,
        HeritageSkillId = milestone.HeritageSkillId,
        HeritageSkillName = milestone.HeritageSkill.Name,
        TargetLevel = milestone.TargetLevel.ToString(),
        DisplayOrder = milestone.DisplayOrder,
        IsCompleted = milestone.IsCompleted,
        CompletedAt = milestone.CompletedAt,
        RecommendedCourses = milestone.RecommendedCourses.OrderBy(c => c.DisplayOrder).Select(c => new RecommendedCourseDto
        {
            CourseId = c.CourseId,
            CourseTitle = c.Course.Title,
            Reason = c.Reason,
        }).ToList(),
        RecommendedLessons = milestone.RecommendedLessons.OrderBy(l => l.DisplayOrder).Select(l => new RecommendedLessonDto
        {
            CourseLessonId = l.CourseLessonId,
            LessonTitle = l.CourseLesson.Title,
            CourseId = l.CourseLesson.CourseId,
            Reason = l.Reason,
        }).ToList(),
    };
}
