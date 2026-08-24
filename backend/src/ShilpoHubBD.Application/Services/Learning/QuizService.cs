using ShilpoHubBD.Application.DTOs.Assessment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Assessment;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public QuizService(
        IQuizRepository quizRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _quizRepository = quizRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<QuizDto> CreateAsync(Guid userId, Guid courseId, CreateQuizRequest request, CancellationToken cancellationToken)
    {
        var course = await GetOwnedCourseAsync(userId, courseId, cancellationToken);

        var now = DateTime.UtcNow;
        var quiz = new Quiz
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

        await _quizRepository.AddAsync(quiz, cancellationToken);
        await _quizRepository.SaveChangesAsync(cancellationToken);

        var created = await _quizRepository.GetByIdAsync(quiz.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<QuizDto> UpdateAsync(Guid userId, Guid quizId, UpdateQuizRequest request, CancellationToken cancellationToken)
    {
        var quiz = await GetOwnedQuizAsync(userId, quizId, cancellationToken);

        quiz.Title = request.Title.Trim();
        quiz.Description = request.Description.Trim();
        quiz.TimeLimitMinutes = request.TimeLimitMinutes;
        quiz.MaxAttempts = request.MaxAttempts;
        quiz.PassingScorePercentage = request.PassingScorePercentage;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.SaveChangesAsync(cancellationToken);
        return ToDto(quiz);
    }

    public async Task DeleteAsync(Guid userId, Guid quizId, CancellationToken cancellationToken)
    {
        var quiz = await GetOwnedQuizAsync(userId, quizId, cancellationToken);

        if (quiz.Attempts.Count > 0)
        {
            throw new ConflictException("This quiz already has attempts and cannot be deleted.");
        }

        _quizRepository.Remove(quiz);
        await _quizRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<QuizDto> GetByIdAsync(Guid userId, Guid quizId, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        await EnsureCanViewCourseAsync(userId, quiz.Course, cancellationToken);
        return ToDto(quiz);
    }

    public async Task<List<QuizListItemDto>> GetByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken)
            ?? throw new NotFoundException("Course not found.");

        await EnsureCanViewCourseAsync(userId, course, cancellationToken);

        var quizzes = await _quizRepository.GetByCourseAsync(courseId, cancellationToken);
        return quizzes.Select(ToListItemDto).ToList();
    }

    public async Task<QuizQuestionDto> AddQuestionAsync(Guid userId, Guid quizId, CreateQuizQuestionRequest request, CancellationToken cancellationToken)
    {
        var quiz = await GetOwnedQuizAsync(userId, quizId, cancellationToken);
        EnsureQuizNotAttempted(quiz);

        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            Body = request.Body.Trim(),
            Points = request.Points,
            DisplayOrder = request.DisplayOrder,
            Options = request.Options.Select(o => new QuizQuestionOption
            {
                Id = Guid.NewGuid(),
                Text = o.Text.Trim(),
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder,
            }).ToList(),
        };

        await _quizRepository.AddQuestionAsync(question, cancellationToken);
        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.SaveChangesAsync(cancellationToken);

        return ToQuestionDto(question);
    }

    public async Task<QuizQuestionDto> UpdateQuestionAsync(
        Guid userId, Guid quizId, Guid questionId, UpdateQuizQuestionRequest request, CancellationToken cancellationToken)
    {
        var quiz = await GetOwnedQuizAsync(userId, quizId, cancellationToken);
        EnsureQuizNotAttempted(quiz);

        var question = await _quizRepository.GetQuestionByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        if (question.QuizId != quizId)
        {
            throw new NotFoundException("Question not found.");
        }

        question.Body = request.Body.Trim();
        question.Points = request.Points;
        question.DisplayOrder = request.DisplayOrder;

        question.Options.Clear();
        foreach (var option in request.Options)
        {
            question.Options.Add(new QuizQuestionOption
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = option.Text.Trim(),
                IsCorrect = option.IsCorrect,
                DisplayOrder = option.DisplayOrder,
            });
        }

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.SaveChangesAsync(cancellationToken);
        return ToQuestionDto(question);
    }

    public async Task DeleteQuestionAsync(Guid userId, Guid quizId, Guid questionId, CancellationToken cancellationToken)
    {
        var quiz = await GetOwnedQuizAsync(userId, quizId, cancellationToken);
        EnsureQuizNotAttempted(quiz);

        var question = await _quizRepository.GetQuestionByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        if (question.QuizId != quizId)
        {
            throw new NotFoundException("Question not found.");
        }

        _quizRepository.RemoveQuestion(question);
        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<QuizAttemptStartDto> StartAttemptAsync(Guid studentUserId, Guid quizId, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        await EnsureEnrolledAsync(studentUserId, quiz.CourseId, cancellationToken);

        if (quiz.Questions.Count == 0)
        {
            throw new ConflictException("This quiz has no questions yet.");
        }

        var priorAttempts = await _quizRepository.GetAttemptsByStudentAsync(quizId, studentUserId, cancellationToken);
        if (priorAttempts.Any(a => a.Status == AttemptStatus.InProgress))
        {
            throw new ConflictException("You already have an attempt in progress for this quiz.");
        }

        if (quiz.MaxAttempts.HasValue && priorAttempts.Count >= quiz.MaxAttempts.Value)
        {
            throw new ConflictException("You have reached the maximum number of attempts for this quiz.");
        }

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            StudentUserId = studentUserId,
            AttemptNumber = priorAttempts.Count + 1,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            MaxScore = quiz.Questions.Sum(q => q.Points),
        };

        await _quizRepository.AddAttemptAsync(attempt, cancellationToken);
        await _quizRepository.SaveChangesAsync(cancellationToken);

        return new QuizAttemptStartDto
        {
            Id = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            Questions = quiz.Questions.OrderBy(q => q.DisplayOrder).Select(ToQuestionForAttemptDto).ToList(),
        };
    }

    public async Task<QuizAttemptResultDto> SubmitAttemptAsync(
        Guid studentUserId, Guid attemptId, SubmitQuizAttemptRequest request, CancellationToken cancellationToken)
    {
        var attempt = await _quizRepository.GetAttemptByIdAsync(attemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.StudentUserId != studentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to submit this attempt.");
        }

        if (attempt.Status != AttemptStatus.InProgress)
        {
            throw new ConflictException("This attempt has already been submitted.");
        }

        var quiz = await _quizRepository.GetByIdAsync(attempt.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        var totalScore = 0;
        foreach (var question in quiz.Questions)
        {
            var submitted = request.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            QuizQuestionOption? selectedOption = null;
            if (submitted?.SelectedOptionId is not null)
            {
                selectedOption = question.Options.FirstOrDefault(o => o.Id == submitted.SelectedOptionId.Value)
                    ?? throw new ConflictException("One of the submitted options does not belong to this quiz.");
            }

            var isCorrect = selectedOption?.IsCorrect ?? false;
            var pointsAwarded = isCorrect ? question.Points : 0;
            totalScore += pointsAwarded;

            var answer = new QuizAttemptAnswer
            {
                Id = Guid.NewGuid(),
                QuizAttemptId = attempt.Id,
                QuizQuestionId = question.Id,
                SelectedOptionId = selectedOption?.Id,
                IsCorrect = isCorrect,
                PointsAwarded = pointsAwarded,
            };

            await _quizRepository.AddAnswerAsync(answer, cancellationToken);
            attempt.Answers.Add(answer);
        }

        attempt.Status = AttemptStatus.Submitted;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.Score = totalScore;
        attempt.PercentageScore = attempt.MaxScore > 0 ? Math.Round(totalScore / (decimal)attempt.MaxScore * 100m, 2) : 0;
        attempt.IsPassed = attempt.PercentageScore >= quiz.PassingScorePercentage;

        await _quizRepository.SaveChangesAsync(cancellationToken);

        var reloaded = await _quizRepository.GetAttemptByIdAsync(attempt.Id, cancellationToken);
        return ToAttemptResultDto(reloaded!);
    }

    public async Task<QuizAttemptResultDto> GetAttemptResultAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _quizRepository.GetAttemptByIdAsync(attemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.StudentUserId != userId && AuthorUserId(attempt.Quiz.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this attempt.");
        }

        if (attempt.Status == AttemptStatus.InProgress)
        {
            throw new ConflictException("This attempt has not been submitted yet.");
        }

        return ToAttemptResultDto(attempt);
    }

    public async Task<List<QuizAttemptListItemDto>> GetMyAttemptsAsync(Guid studentUserId, Guid quizId, CancellationToken cancellationToken)
    {
        var attempts = await _quizRepository.GetAttemptsByStudentAsync(quizId, studentUserId, cancellationToken);
        return attempts.Select(ToAttemptListItemDto).ToList();
    }

    public async Task<List<QuizAttemptListItemDto>> GetAttemptsForTrainerAsync(Guid userId, Guid quizId, CancellationToken cancellationToken)
    {
        await GetOwnedQuizAsync(userId, quizId, cancellationToken);

        var attempts = await _quizRepository.GetAttemptsByQuizAsync(quizId, cancellationToken);
        return attempts.Select(ToAttemptListItemDto).ToList();
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

    private async Task<Quiz> GetOwnedQuizAsync(Guid userId, Guid quizId, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        if (AuthorUserId(quiz.Course) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this quiz.");
        }

        return quiz;
    }

    private static void EnsureQuizNotAttempted(Quiz quiz)
    {
        if (quiz.Attempts.Count > 0)
        {
            throw new ConflictException("Questions cannot be changed after students have started attempting this quiz.");
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
            throw new UnauthorizedAccessException("You do not have permission to view this course's quizzes.");
        }
    }

    private static Guid? AuthorUserId(Course course)
        => course.Mentor?.UserId ?? course.TrainerProfile?.UserId;

    private static QuizListItemDto ToListItemDto(Quiz quiz) => new()
    {
        Id = quiz.Id,
        CourseId = quiz.CourseId,
        CourseTitle = quiz.Course.Title,
        Title = quiz.Title,
        TimeLimitMinutes = quiz.TimeLimitMinutes,
        MaxAttempts = quiz.MaxAttempts,
        PassingScorePercentage = quiz.PassingScorePercentage,
        QuestionCount = quiz.Questions.Count,
        TotalPoints = quiz.Questions.Sum(q => q.Points),
    };

    private static QuizDto ToDto(Quiz quiz) => new()
    {
        Id = quiz.Id,
        CourseId = quiz.CourseId,
        CourseTitle = quiz.Course.Title,
        Title = quiz.Title,
        Description = quiz.Description,
        TimeLimitMinutes = quiz.TimeLimitMinutes,
        MaxAttempts = quiz.MaxAttempts,
        PassingScorePercentage = quiz.PassingScorePercentage,
        TotalPoints = quiz.Questions.Sum(q => q.Points),
        Questions = quiz.Questions.OrderBy(q => q.DisplayOrder).Select(ToQuestionDto).ToList(),
        CreatedAt = quiz.CreatedAt,
        UpdatedAt = quiz.UpdatedAt,
    };

    private static QuizQuestionDto ToQuestionDto(QuizQuestion question) => new()
    {
        Id = question.Id,
        QuizId = question.QuizId,
        Body = question.Body,
        Points = question.Points,
        DisplayOrder = question.DisplayOrder,
        Options = question.Options.OrderBy(o => o.DisplayOrder).Select(o => new QuizQuestionOptionDto
        {
            Id = o.Id,
            Text = o.Text,
            IsCorrect = o.IsCorrect,
            DisplayOrder = o.DisplayOrder,
        }).ToList(),
    };

    private static QuizQuestionForAttemptDto ToQuestionForAttemptDto(QuizQuestion question) => new()
    {
        Id = question.Id,
        Body = question.Body,
        Points = question.Points,
        DisplayOrder = question.DisplayOrder,
        Options = question.Options.OrderBy(o => o.DisplayOrder).Select(o => new QuizQuestionOptionForAttemptDto
        {
            Id = o.Id,
            Text = o.Text,
            DisplayOrder = o.DisplayOrder,
        }).ToList(),
    };

    private static QuizAttemptListItemDto ToAttemptListItemDto(QuizAttempt attempt) => new()
    {
        Id = attempt.Id,
        QuizId = attempt.QuizId,
        QuizTitle = attempt.Quiz.Title,
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

    private static QuizAttemptResultDto ToAttemptResultDto(QuizAttempt attempt) => new()
    {
        Id = attempt.Id,
        QuizId = attempt.QuizId,
        QuizTitle = attempt.Quiz.Title,
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
        Answers = attempt.Answers.Select(a =>
        {
            var correctOption = a.QuizQuestion.Options.FirstOrDefault(o => o.IsCorrect);
            return new QuizAttemptAnswerDto
            {
                QuestionId = a.QuizQuestionId,
                QuestionBody = a.QuizQuestion.Body,
                SelectedOptionId = a.SelectedOptionId,
                SelectedOptionText = a.SelectedOption?.Text,
                CorrectOptionId = correctOption?.Id,
                CorrectOptionText = correctOption?.Text,
                IsCorrect = a.IsCorrect,
                PointsAwarded = a.PointsAwarded,
            };
        }).ToList(),
    };
}
