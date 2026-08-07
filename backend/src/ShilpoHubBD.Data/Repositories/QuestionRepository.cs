using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly ShilpoHubDbContext _context;

    public QuestionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<CommunityQuestion> WithDetails()
        => _context.CommunityQuestions
            .Include(q => q.User)
            .Include(q => q.Answers).ThenInclude(a => a.User)
            .AsSplitQuery();

    public async Task<(List<CommunityQuestion> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var questions = WithDetails().Where(q => q.ProductId == productId).OrderByDescending(q => q.CreatedAt);

        var totalCount = await questions.CountAsync(cancellationToken);
        var items = await questions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<CommunityQuestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task AddAsync(CommunityQuestion question, CancellationToken cancellationToken)
        => await _context.CommunityQuestions.AddAsync(question, cancellationToken);

    public async Task AddAnswerAsync(CommunityAnswer answer, CancellationToken cancellationToken)
        => await _context.CommunityAnswers.AddAsync(answer, cancellationToken);

    public void RemoveAnswer(CommunityAnswer answer)
        => _context.CommunityAnswers.Remove(answer);

    public void Remove(CommunityQuestion question)
        => _context.CommunityQuestions.Remove(question);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
