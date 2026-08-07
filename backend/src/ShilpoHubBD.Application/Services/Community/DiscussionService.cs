using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Services.Community;

public class DiscussionService : IDiscussionService
{
    private readonly IDiscussionRepository _discussionRepository;

    public DiscussionService(IDiscussionRepository discussionRepository)
    {
        _discussionRepository = discussionRepository;
    }

    public async Task<PagedResult<DiscussionThreadListItemDto>> GetAllAsync(DiscussionQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _discussionRepository.GetPagedAsync(query.Category, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<DiscussionThreadListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<DiscussionThreadDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var thread = await _discussionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Discussion thread not found.");

        return ToDto(thread);
    }

    public async Task<DiscussionThreadDto> CreateAsync(Guid userId, CreateDiscussionThreadRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var thread = new DiscussionThread
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Category = request.Category.Trim(),
            Body = request.Body.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _discussionRepository.AddAsync(thread, cancellationToken);
        await _discussionRepository.SaveChangesAsync(cancellationToken);

        var created = await _discussionRepository.GetByIdAsync(thread.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<DiscussionThreadDto> ReplyAsync(Guid threadId, Guid userId, CreateDiscussionReplyRequest request, CancellationToken cancellationToken)
    {
        var thread = await _discussionRepository.GetByIdAsync(threadId, cancellationToken)
            ?? throw new NotFoundException("Discussion thread not found.");

        var reply = new DiscussionReply
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            UserId = userId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _discussionRepository.AddReplyAsync(reply, cancellationToken);
        await _discussionRepository.SaveChangesAsync(cancellationToken);

        var updated = await _discussionRepository.GetByIdAsync(threadId, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteThreadAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var thread = await _discussionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Discussion thread not found.");

        if (!isAdmin && thread.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this thread.");
        }

        _discussionRepository.Remove(thread);
        await _discussionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteReplyAsync(Guid threadId, Guid replyId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var thread = await _discussionRepository.GetByIdAsync(threadId, cancellationToken)
            ?? throw new NotFoundException("Discussion thread not found.");

        var reply = thread.Replies.FirstOrDefault(r => r.Id == replyId)
            ?? throw new NotFoundException("Reply not found.");

        if (!isAdmin && reply.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this reply.");
        }

        _discussionRepository.RemoveReply(reply);
        await _discussionRepository.SaveChangesAsync(cancellationToken);
    }

    private static DiscussionThreadListItemDto ToListItemDto(DiscussionThread thread) => new()
    {
        Id = thread.Id,
        Title = thread.Title,
        Category = thread.Category,
        AuthorName = thread.User.FullName,
        ReplyCount = thread.Replies.Count,
        CreatedAt = thread.CreatedAt,
    };

    private static DiscussionThreadDto ToDto(DiscussionThread thread) => new()
    {
        Id = thread.Id,
        UserId = thread.UserId,
        Title = thread.Title,
        Category = thread.Category,
        Body = thread.Body,
        AuthorName = thread.User.FullName,
        CreatedAt = thread.CreatedAt,
        UpdatedAt = thread.UpdatedAt,
        Replies = thread.Replies
            .OrderBy(r => r.CreatedAt)
            .Select(r => new DiscussionReplyDto
            {
                Id = r.Id,
                UserId = r.UserId,
                AuthorName = r.User.FullName,
                Body = r.Body,
                CreatedAt = r.CreatedAt,
            })
            .ToList(),
    };
}
