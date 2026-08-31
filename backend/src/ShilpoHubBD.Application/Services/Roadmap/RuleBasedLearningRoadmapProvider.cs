using ShilpoHubBD.Application.DTOs.Roadmap;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Roadmap;

// Rule-based stand-in for a future AI-backed roadmap planner. No external model calls — milestone
// selection and course/lesson picks are derived purely from the learner's current skills and the
// candidate pool supplied in the input.
public class RuleBasedLearningRoadmapProvider : ILearningRoadmapProvider
{
    private const int MaxRecommendedCoursesPerMilestone = 2;
    private const int MaxRecommendedLessonsPerMilestone = 2;
    private const int MaxPrerequisiteMilestones = 2;
    private const int MaxSkillMilestones = 4;

    public Task<RoadmapGenerationResult> GenerateAsync(RoadmapGenerationInput input, CancellationToken cancellationToken)
    {
        var planned = new List<(Guid SkillId, SkillLevel TargetLevel)>();

        if (input.TargetHeritageSkillId.HasValue)
        {
            var prerequisites = input.CurrentSkills
                .Where(s => s.HeritageSkillId != input.TargetHeritageSkillId.Value && (s.CurrentLevel ?? SkillLevel.Beginner) < SkillLevel.Advanced)
                .OrderBy(s => s.CurrentLevel ?? SkillLevel.Beginner)
                .Take(MaxPrerequisiteMilestones);

            foreach (var prerequisite in prerequisites)
            {
                planned.Add((prerequisite.HeritageSkillId, NextLevel(prerequisite.CurrentLevel ?? SkillLevel.Beginner)));
            }

            var targetCurrentLevel = input.CurrentSkills
                .FirstOrDefault(s => s.HeritageSkillId == input.TargetHeritageSkillId.Value)?.CurrentLevel;
            planned.Add((input.TargetHeritageSkillId.Value, NextLevel(targetCurrentLevel ?? SkillLevel.Beginner)));
        }
        else
        {
            var activeSkills = input.CurrentSkills
                .Where(s => (s.CurrentLevel ?? SkillLevel.Beginner) < SkillLevel.Expert)
                .OrderBy(s => s.CurrentLevel ?? SkillLevel.Beginner)
                .Take(MaxSkillMilestones);

            foreach (var skill in activeSkills)
            {
                planned.Add((skill.HeritageSkillId, NextLevel(skill.CurrentLevel ?? SkillLevel.Beginner)));
            }

            var newSkill = input.CandidateSkills.FirstOrDefault(c => input.CurrentSkills.All(s => s.HeritageSkillId != c.HeritageSkillId));
            if (newSkill is not null)
            {
                planned.Add((newSkill.HeritageSkillId, SkillLevel.Intermediate));
            }
        }

        if (planned.Count == 0 && input.CandidateSkills.Count > 0)
        {
            planned.Add((input.CandidateSkills[0].HeritageSkillId, SkillLevel.Intermediate));
        }

        var milestones = planned.Select(p => BuildMilestone(p.SkillId, p.TargetLevel, input)).ToList();
        return Task.FromResult(new RoadmapGenerationResult { Milestones = milestones });
    }

    private static MilestonePlanResult BuildMilestone(Guid skillId, SkillLevel targetLevel, RoadmapGenerationInput input)
    {
        var planningContext = input.CandidateSkills.FirstOrDefault(c => c.HeritageSkillId == skillId);
        var currentLevel = input.CurrentSkills.FirstOrDefault(s => s.HeritageSkillId == skillId)?.CurrentLevel;

        var recommendedCourses = new List<RecommendedCoursePlan>();
        var recommendedLessons = new List<RecommendedLessonPlan>();

        if (planningContext is not null)
        {
            var topCourses = planningContext.Courses.Take(MaxRecommendedCoursesPerMilestone).ToList();
            foreach (var course in topCourses)
            {
                recommendedCourses.Add(new RecommendedCoursePlan
                {
                    CourseId = course.CourseId,
                    Reason = $"Builds practical skills toward reaching {targetLevel} in {planningContext.Name}.",
                });
            }

            var firstCourse = topCourses.FirstOrDefault();
            if (firstCourse is not null)
            {
                foreach (var lesson in firstCourse.Lessons.Take(MaxRecommendedLessonsPerMilestone))
                {
                    recommendedLessons.Add(new RecommendedLessonPlan
                    {
                        CourseLessonId = lesson.CourseLessonId,
                        Reason = $"Foundational lesson for {planningContext.Name}.",
                    });
                }
            }
        }

        return new MilestonePlanResult
        {
            HeritageSkillId = skillId,
            TargetLevel = targetLevel,
            IsAlreadyCompleted = (currentLevel ?? SkillLevel.Beginner) >= targetLevel && currentLevel.HasValue,
            RecommendedCourses = recommendedCourses,
            RecommendedLessons = recommendedLessons,
        };
    }

    private static SkillLevel NextLevel(SkillLevel level) => level switch
    {
        SkillLevel.Beginner => SkillLevel.Intermediate,
        SkillLevel.Intermediate => SkillLevel.Advanced,
        SkillLevel.Advanced => SkillLevel.Expert,
        _ => SkillLevel.Expert,
    };
}
