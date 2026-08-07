using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IQuestionService
{
    Task<PagedResult<QuestionDto>> GetByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<QuestionDto> AskAsync(Guid productId, Guid userId, CreateQuestionRequest request, CancellationToken cancellationToken);
    Task<QuestionDto> AnswerAsync(Guid questionId, Guid userId, CreateAnswerRequest request, CancellationToken cancellationToken);
    Task DeleteQuestionAsync(Guid questionId, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task DeleteAnswerAsync(Guid questionId, Guid answerId, Guid userId, bool isAdmin, CancellationToken cancellationToken);
}
