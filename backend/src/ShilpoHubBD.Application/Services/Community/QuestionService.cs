using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Services.Community;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IProductRepository _productRepository;

    public QuestionService(IQuestionRepository questionRepository, IProductRepository productRepository)
    {
        _questionRepository = questionRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResult<QuestionDto>> GetByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _questionRepository.GetPagedByProductAsync(productId, page, pageSize, cancellationToken);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        return new PagedResult<QuestionDto>
        {
            Items = items.Select(q => ToDto(q, product?.ProducerId)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<QuestionDto> AskAsync(Guid productId, Guid userId, CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var question = new CommunityQuestion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _questionRepository.AddAsync(question, cancellationToken);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        var created = await _questionRepository.GetByIdAsync(question.Id, cancellationToken);
        return ToDto(created!, product.ProducerId);
    }

    public async Task<QuestionDto> AnswerAsync(Guid questionId, Guid userId, CreateAnswerRequest request, CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        var answer = new CommunityAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            UserId = userId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _questionRepository.AddAnswerAsync(answer, cancellationToken);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        var product = await _productRepository.GetByIdAsync(question.ProductId, cancellationToken);
        var updated = await _questionRepository.GetByIdAsync(questionId, cancellationToken);
        return ToDto(updated!, product?.ProducerId);
    }

    public async Task DeleteQuestionAsync(Guid questionId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        if (!isAdmin && question.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this question.");
        }

        _questionRepository.Remove(question);
        await _questionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAnswerAsync(Guid questionId, Guid answerId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        var answer = question.Answers.FirstOrDefault(a => a.Id == answerId)
            ?? throw new NotFoundException("Answer not found.");

        if (!isAdmin && answer.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this answer.");
        }

        _questionRepository.RemoveAnswer(answer);
        await _questionRepository.SaveChangesAsync(cancellationToken);
    }

    private static QuestionDto ToDto(CommunityQuestion question, Guid? producerId) => new()
    {
        Id = question.Id,
        ProductId = question.ProductId,
        UserId = question.UserId,
        AskerName = question.User.FullName,
        Body = question.Body,
        CreatedAt = question.CreatedAt,
        Answers = question.Answers
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AnswerDto
            {
                Id = a.Id,
                UserId = a.UserId,
                AuthorName = a.User.FullName,
                IsProducerAnswer = producerId.HasValue && a.UserId == producerId.Value,
                Body = a.Body,
                CreatedAt = a.CreatedAt,
            })
            .ToList(),
    };
}
