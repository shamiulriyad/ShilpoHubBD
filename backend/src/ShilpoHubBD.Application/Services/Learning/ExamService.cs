using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Assessment;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ExamService(
        IExamRepository examRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _examRepository = examRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ExamDto> CreateAsync(Guid userId, Guid courseId, CreateExamRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var now = DateTime.UtcNow;
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = request.MaxAttempts,
            PassingScorePercentage = request.PassingScorePercentage,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _examRepository.AddAsync(exam, cancellationToken);
        await _examRepository.SaveChangesAsync(cancellationToken);

        var created = await _examRepository.GetByIdAsync(exam.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<ExamDto> UpdateAsync(Guid userId, Guid examId, UpdateExamRequest request, CancellationToken cancellationToken)
    {
        var exam = await GetOwnedExamAsync(userId, examId, cancellationToken);

        exam.Title = request.Title.Trim();
        exam.Description = request.Description.Trim();
        exam.TimeLimitMinutes = request.TimeLimitMinutes;
        exam.MaxAttempts = request.MaxAttempts;
        exam.PassingScorePercentage = request.PassingScorePercentage;
        exam.UpdatedAt = DateTime.UtcNow;

        await _examRepository.SaveChangesAsync(cancellationToken);
        return ToDto(exam);
    }

    public async Task DeleteAsync(Guid userId, Guid examId, CancellationToken cancellationToken)
    {
        var exam = await GetOwnedExamAsync(userId, examId, cancellationToken);

        if (exam.Attempts.Count > 0)
        {
            throw new ConflictException("This exam already has attempts and cannot be deleted.");
        }

        _examRepository.Remove(exam);
        await _examRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExamDto> GetByIdAsync(Guid userId, Guid examId, CancellationToken cancellationToken)
    {
        var exam = await _examRepository.GetByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.");

        await EnsureCanViewCourseAsync(userId, exam.Course, cancellationToken);
        return ToDto(exam);
    }

    public async Task<List<ExamListItemDto>> GetByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        await EnsureCanViewCourseAsync(userId, course, cancellationToken);

        var exams = await _examRepository.GetByCourseAsync(courseId, cancellationToken);
        return exams.Select(ToListItemDto).ToList();
    }

    public async Task<ExamQuestionDto> AddQuestionAsync(Guid userId, Guid examId, CreateExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var exam = await GetOwnedExamAsync(userId, examId, cancellationToken);
        EnsureExamNotAttempted(exam);

        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            Body = request.Body.Trim(),
            QuestionType = request.QuestionType,
            Points = request.Points,
            DisplayOrder = request.DisplayOrder,
            Options = request.QuestionType == QuestionType.MultipleChoice
                ? request.Options.Select(o => new ExamQuestionOption
                {
                    Id = Guid.NewGuid(),
                    Text = o.Text.Trim(),
                    IsCorrect = o.IsCorrect,
                    DisplayOrder = o.DisplayOrder,
                }).ToList()
                : new List<ExamQuestionOption>(),
        };

        await _examRepository.AddQuestionAsync(question, cancellationToken);
        exam.UpdatedAt = DateTime.UtcNow;
        await _examRepository.SaveChangesAsync(cancellationToken);

        return ToQuestionDto(question);
    }

    public async Task<ExamQuestionDto> UpdateQuestionAsync(
        Guid userId, Guid examId, Guid questionId, UpdateExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var exam = await GetOwnedExamAsync(userId, examId, cancellationToken);
        EnsureExamNotAttempted(exam);

        var question = await _examRepository.GetQuestionByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        if (question.ExamId != examId)
        {
            throw new NotFoundException("Question not found.");
        }

        question.Body = request.Body.Trim();
        question.QuestionType = request.QuestionType;
        question.Points = request.Points;
        question.DisplayOrder = request.DisplayOrder;

        question.Options.Clear();
        if (request.QuestionType == QuestionType.MultipleChoice)
        {
            foreach (var option in request.Options)
            {
                question.Options.Add(new ExamQuestionOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Text = option.Text.Trim(),
                    IsCorrect = option.IsCorrect,
                    DisplayOrder = option.DisplayOrder,
                });
            }
        }

        exam.UpdatedAt = DateTime.UtcNow;
        await _examRepository.SaveChangesAsync(cancellationToken);
        return ToQuestionDto(question);
    }

    public async Task DeleteQuestionAsync(Guid userId, Guid examId, Guid questionId, CancellationToken cancellationToken)
    {
        var exam = await GetOwnedExamAsync(userId, examId, cancellationToken);
        EnsureExamNotAttempted(exam);

        var question = await _examRepository.GetQuestionByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        if (question.ExamId != examId)
        {
            throw new NotFoundException("Question not found.");
        }

        _examRepository.RemoveQuestion(question);
        exam.UpdatedAt = DateTime.UtcNow;
        await _examRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExamAttemptStartDto> StartAttemptAsync(Guid studentUserId, Guid examId, CancellationToken cancellationToken)
    {
        var exam = await _examRepository.GetByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.");

        await EnsureEnrolledAsync(studentUserId, exam.CourseId, cancellationToken);

        if (exam.Questions.Count == 0)
        {
            throw new ConflictException("This exam has no questions yet.");
        }

        var priorAttempts = await _examRepository.GetAttemptsByStudentAsync(examId, studentUserId, cancellationToken);
        if (priorAttempts.Any(a => a.Status == AttemptStatus.InProgress))
        {
            throw new ConflictException("You already have an attempt in progress for this exam.");
        }

        if (exam.MaxAttempts.HasValue && priorAttempts.Count >= exam.MaxAttempts.Value)
        {
            throw new ConflictException("You have reached the maximum number of attempts for this exam.");
        }

        var attempt = new ExamAttempt
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            StudentUserId = studentUserId,
            AttemptNumber = priorAttempts.Count + 1,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            MaxScore = exam.Questions.Sum(q => q.Points),
        };

        await _examRepository.AddAttemptAsync(attempt, cancellationToken);
        await _examRepository.SaveChangesAsync(cancellationToken);

        return new ExamAttemptStartDto
        {
            Id = attempt.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            TimeLimitMinutes = exam.TimeLimitMinutes,
            Questions = exam.Questions.OrderBy(q => q.DisplayOrder).Select(ToQuestionForAttemptDto).ToList(),
        };
    }

    public async Task<ExamAttemptResultDto> SubmitAttemptAsync(
        Guid studentUserId, Guid attemptId, SubmitExamAttemptRequest request, CancellationToken cancellationToken)
    {
        var attempt = await _examRepository.GetAttemptByIdAsync(attemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.StudentUserId != studentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to submit this attempt.");
        }

        if (attempt.Status != AttemptStatus.InProgress)
        {
            throw new ConflictException("This attempt has already been submitted.");
        }

        var exam = await _examRepository.GetByIdAsync(attempt.ExamId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.");

        var autoScore = 0;
        var hasEssayQuestions = exam.Questions.Any(q => q.QuestionType == QuestionType.Essay);

        foreach (var question in exam.Questions)
        {
            var submitted = request.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            var answer = new ExamAttemptAnswer
            {
                Id = Guid.NewGuid(),
                ExamAttemptId = attempt.Id,
                ExamQuestionId = question.Id,
            };

            if (question.QuestionType == QuestionType.MultipleChoice)
            {
                ExamQuestionOption? selectedOption = null;
                if (submitted?.SelectedOptionId is not null)
                {
                    selectedOption = question.Options.FirstOrDefault(o => o.Id == submitted.SelectedOptionId.Value)
                        ?? throw new ConflictException("One of the submitted options does not belong to this exam.");
                }

                var isCorrect = selectedOption?.IsCorrect ?? false;
                var pointsAwarded = isCorrect ? question.Points : 0;
                autoScore += pointsAwarded;

                answer.SelectedOptionId = selectedOption?.Id;
                answer.IsCorrect = isCorrect;
                answer.PointsAwarded = pointsAwarded;
            }
            else
            {
                answer.EssayAnswerText = submitted?.EssayAnswerText?.Trim();
            }

            await _examRepository.AddAnswerAsync(answer, cancellationToken);
            attempt.Answers.Add(answer);
        }

        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.Score = autoScore;

        if (hasEssayQuestions)
        {
            attempt.Status = AttemptStatus.Submitted;
        }
        else
        {
            attempt.Status = AttemptStatus.Evaluated;
            attempt.PercentageScore = attempt.MaxScore > 0 ? Math.Round(autoScore / (decimal)attempt.MaxScore * 100m, 2) : 0;
            attempt.IsPassed = attempt.PercentageScore >= exam.PassingScorePercentage;
            attempt.EvaluatedAt = attempt.SubmittedAt;
        }

        await _examRepository.SaveChangesAsync(cancellationToken);

        var reloaded = await _examRepository.GetAttemptByIdAsync(attempt.Id, cancellationToken);
        return ToAttemptResultDto(reloaded!);
    }

    public async Task<ExamAttemptResultDto> GetAttemptResultAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _examRepository.GetAttemptByIdAsync(attemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.StudentUserId != userId && AuthorUserId(attempt.Exam.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this attempt.");
        }

        if (attempt.Status == AttemptStatus.InProgress)
        {
            throw new ConflictException("This attempt has not been submitted yet.");
        }

        return ToAttemptResultDto(attempt);
    }

    public async Task<List<ExamAttemptListItemDto>> GetMyAttemptsAsync(Guid studentUserId, Guid examId, CancellationToken cancellationToken)
    {
        var attempts = await _examRepository.GetAttemptsByStudentAsync(examId, studentUserId, cancellationToken);
        return attempts.Select(ToAttemptListItemDto).ToList();
    }

    public async Task<List<ExamAttemptListItemDto>> GetAttemptsForTrainerAsync(Guid userId, Guid examId, CancellationToken cancellationToken)
    {
        await GetOwnedExamAsync(userId, examId, cancellationToken);

        var attempts = await _examRepository.GetAttemptsByExamAsync(examId, cancellationToken);
        return attempts.Select(ToAttemptListItemDto).ToList();
    }

    public async Task<ExamAttemptResultDto> EvaluateAnswerAsync(
        Guid userId, Guid attemptId, Guid questionId, EvaluateExamAnswerRequest request, CancellationToken cancellationToken)
    {
        var attempt = await _examRepository.GetAttemptByIdAsync(attemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (AuthorUserId(attempt.Exam.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to evaluate this attempt.");
        }

        if (attempt.Status != AttemptStatus.Submitted)
        {
            throw new ConflictException("This attempt is not awaiting manual evaluation.");
        }

        var answer = attempt.Answers.FirstOrDefault(a => a.ExamQuestionId == questionId)
            ?? throw new NotFoundException("Answer not found.");

        if (answer.ExamQuestion.QuestionType != QuestionType.Essay)
        {
            throw new ConflictException("Only essay answers require manual evaluation.");
        }

        if (request.PointsAwarded < 0 || request.PointsAwarded > answer.ExamQuestion.Points)
        {
            throw new ConflictException($"Points awarded must be between 0 and {answer.ExamQuestion.Points}.");
        }

        answer.PointsAwarded = request.PointsAwarded;
        answer.Feedback = request.Feedback?.Trim();

        await _examRepository.SaveChangesAsync(cancellationToken);
        return ToAttemptResultDto(attempt);
    }

    public async Task<ExamAttemptResultDto> FinalizeEvaluationAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _examRepository.GetAttemptByIdAsync(attemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (AuthorUserId(attempt.Exam.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to evaluate this attempt.");
        }

        if (attempt.Status != AttemptStatus.Submitted)
        {
            throw new ConflictException("This attempt is not awaiting manual evaluation.");
        }

        var essayAnswers = attempt.Answers.Where(a => a.ExamQuestion.QuestionType == QuestionType.Essay).ToList();
        if (essayAnswers.Any(a => a.PointsAwarded is null))
        {
            throw new ConflictException("All essay questions must be evaluated before finalizing this attempt.");
        }

        var totalScore = attempt.Answers.Sum(a => a.PointsAwarded ?? 0);

        attempt.Score = totalScore;
        attempt.PercentageScore = attempt.MaxScore > 0 ? Math.Round(totalScore / (decimal)attempt.MaxScore * 100m, 2) : 0;
        attempt.IsPassed = attempt.PercentageScore >= attempt.Exam.PassingScorePercentage;
        attempt.Status = AttemptStatus.Evaluated;
        attempt.EvaluatedAt = DateTime.UtcNow;
        attempt.EvaluatedByUserId = userId;

        await _examRepository.SaveChangesAsync(cancellationToken);
        return ToAttemptResultDto(attempt);
    }

    private async Task<Course> GetOwnedCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        if (AuthorUserId(course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this course.");
        }

        return course;
    }

    private async Task<Exam> GetOwnedExamAsync(Guid userId, Guid examId, CancellationToken cancellationToken)
    {
        var exam = await _examRepository.GetByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException("Exam not found.");

        if (AuthorUserId(exam.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this exam.");
        }

        return exam;
    }

    private static void EnsureExamNotAttempted(Exam exam)
    {
        if (exam.Attempts.Count > 0)
        {
            throw new ConflictException("Questions cannot be changed after students have started attempting this exam.");
        }
    }

    private async Task EnsureEnrolledAsync(Guid studentUserId, Guid courseId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByCourseAndApprenticeAsync(courseId, studentUserId, cancellationToken);
        if (enrollment is null || enrollment.Status != EnrollmentStatus.Active)
        {
            throw new ConflictException("You must be actively enrolled in this course.");
        }
    }

    private async Task EnsureCanViewCourseAsync(Guid userId, Course course, CancellationToken cancellationToken)
    {
        if (AuthorUserId(course) == userId)
        {
            return;
        }

        var enrollment = await _enrollmentRepository.GetByCourseAndApprenticeAsync(course.Id, userId, cancellationToken);
        if (enrollment is null)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this course's exams.");
        }
    }

    private static Guid? AuthorUserId(Course course)
        => course.Mentor?.UserId ?? course.TrainerProfile?.UserId;

    private static ExamListItemDto ToListItemDto(Exam exam) => new()
    {
        Id = exam.Id,
        CourseId = exam.CourseId,
        CourseTitle = exam.Course.Title,
        Title = exam.Title,
        TimeLimitMinutes = exam.TimeLimitMinutes,
        MaxAttempts = exam.MaxAttempts,
        PassingScorePercentage = exam.PassingScorePercentage,
        QuestionCount = exam.Questions.Count,
        TotalPoints = exam.Questions.Sum(q => q.Points),
    };

    private static ExamDto ToDto(Exam exam) => new()
    {
        Id = exam.Id,
        CourseId = exam.CourseId,
        CourseTitle = exam.Course.Title,
        Title = exam.Title,
        Description = exam.Description,
        TimeLimitMinutes = exam.TimeLimitMinutes,
        MaxAttempts = exam.MaxAttempts,
        PassingScorePercentage = exam.PassingScorePercentage,
        TotalPoints = exam.Questions.Sum(q => q.Points),
        Questions = exam.Questions.OrderBy(q => q.DisplayOrder).Select(ToQuestionDto).ToList(),
        CreatedAt = exam.CreatedAt,
        UpdatedAt = exam.UpdatedAt,
    };

    private static ExamQuestionDto ToQuestionDto(ExamQuestion question) => new()
    {
        Id = question.Id,
        ExamId = question.ExamId,
        Body = question.Body,
        QuestionType = question.QuestionType.ToString(),
        Points = question.Points,
        DisplayOrder = question.DisplayOrder,
        Options = question.Options.OrderBy(o => o.DisplayOrder).Select(o => new ExamQuestionOptionDto
        {
            Id = o.Id,
            Text = o.Text,
            IsCorrect = o.IsCorrect,
            DisplayOrder = o.DisplayOrder,
        }).ToList(),
    };

    private static ExamQuestionForAttemptDto ToQuestionForAttemptDto(ExamQuestion question) => new()
    {
        Id = question.Id,
        Body = question.Body,
        QuestionType = question.QuestionType.ToString(),
        Points = question.Points,
        DisplayOrder = question.DisplayOrder,
        Options = question.Options.OrderBy(o => o.DisplayOrder).Select(o => new ExamQuestionOptionForAttemptDto
        {
            Id = o.Id,
            Text = o.Text,
            DisplayOrder = o.DisplayOrder,
        }).ToList(),
    };

    private static ExamAttemptListItemDto ToAttemptListItemDto(ExamAttempt attempt) => new()
    {
        Id = attempt.Id,
        ExamId = attempt.ExamId,
        ExamTitle = attempt.Exam.Title,
        StudentUserId = attempt.StudentUserId,
        StudentName = attempt.Student.FullName,
        AttemptNumber = attempt.AttemptNumber,
        Status = attempt.Status.ToString(),
        Score = attempt.Score,
        MaxScore = attempt.MaxScore,
        PercentageScore = attempt.PercentageScore,
        IsPassed = attempt.IsPassed,
        StartedAt = attempt.StartedAt,
        SubmittedAt = attempt.SubmittedAt,
    };

    private static ExamAttemptResultDto ToAttemptResultDto(ExamAttempt attempt) => new()
    {
        Id = attempt.Id,
        ExamId = attempt.ExamId,
        ExamTitle = attempt.Exam.Title,
        StudentUserId = attempt.StudentUserId,
        StudentName = attempt.Student.FullName,
        AttemptNumber = attempt.AttemptNumber,
        Status = attempt.Status.ToString(),
        StartedAt = attempt.StartedAt,
        SubmittedAt = attempt.SubmittedAt,
        Score = attempt.Score,
        MaxScore = attempt.MaxScore,
        PercentageScore = attempt.PercentageScore,
        IsPassed = attempt.IsPassed,
        EvaluatedAt = attempt.EvaluatedAt,
        Answers = attempt.Answers.Select(a =>
        {
            var correctOption = a.ExamQuestion.Options.FirstOrDefault(o => o.IsCorrect);
            return new ExamAttemptAnswerDto
            {
                QuestionId = a.ExamQuestionId,
                QuestionBody = a.ExamQuestion.Body,
                QuestionType = a.ExamQuestion.QuestionType.ToString(),
                SelectedOptionId = a.SelectedOptionId,
                SelectedOptionText = a.SelectedOption?.Text,
                CorrectOptionId = correctOption?.Id,
                CorrectOptionText = correctOption?.Text,
                EssayAnswerText = a.EssayAnswerText,
                IsCorrect = a.IsCorrect,
                PointsAwarded = a.PointsAwarded,
                Feedback = a.Feedback,
            };
        }).ToList(),
    };
}
