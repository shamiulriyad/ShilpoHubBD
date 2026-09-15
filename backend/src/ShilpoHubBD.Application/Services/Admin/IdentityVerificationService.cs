using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Admin;

namespace ShilpoHubBD.Application.Services.Admin;

/// <summary>Government-ID / business-document verification requests, submitted by users and reviewed by Super Admin.</summary>
public class IdentityVerificationService : IIdentityVerificationService
{
    private readonly IIdentityVerificationRepository _repository;

    public IdentityVerificationService(IIdentityVerificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IdentityVerificationDto> SubmitAsync(
        Guid userId, SubmitIdentityVerificationRequest request, CancellationToken cancellationToken)
    {
        var type = ParseEnum<IdentityVerificationType>(request.Type, "Invalid Type.");

        if (await _repository.HasPendingRequestAsync(userId, cancellationToken))
        {
            throw new ConflictException("You already have a pending identity verification request.");
        }

        var now = DateTime.UtcNow;
        var verification = new IdentityVerificationRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Status = IdentityVerificationStatus.Pending,
            DocumentNumber = request.DocumentNumber.Trim(),
            FrontImageUrl = request.FrontImageUrl.Trim(),
            BackImageUrl = string.IsNullOrWhiteSpace(request.BackImageUrl) ? null : request.BackImageUrl.Trim(),
            SelfieImageUrl = string.IsNullOrWhiteSpace(request.SelfieImageUrl) ? null : request.SelfieImageUrl.Trim(),
            ApplicantNote = string.IsNullOrWhiteSpace(request.ApplicantNote) ? null : request.ApplicantNote.Trim(),
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(verification, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(verification.Id, cancellationToken))!.ToDto();
    }

    public async Task<List<IdentityVerificationDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
        => (await _repository.GetByUserIdAsync(userId, cancellationToken)).Select(v => v.ToDto()).ToList();

    public async Task<PagedResult<IdentityVerificationDto>> GetPagedAsync(
        IdentityVerificationQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<IdentityVerificationDto>
        {
            Items = items.Select(v => v.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<IdentityVerificationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadAsync(id, cancellationToken)).ToDto();

    public async Task<IdentityVerificationDto> ApproveAsync(Guid id, Guid reviewerUserId, CancellationToken cancellationToken)
    {
        var verification = await LoadAsync(id, cancellationToken);
        EnsurePending(verification);

        verification.Status = IdentityVerificationStatus.Approved;
        verification.ReviewedAt = DateTime.UtcNow;
        verification.ReviewedByUserId = reviewerUserId;
        verification.RejectionReason = null;
        verification.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<IdentityVerificationDto> RejectAsync(
        Guid id, Guid reviewerUserId, RejectIdentityVerificationRequest request, CancellationToken cancellationToken)
    {
        var verification = await LoadAsync(id, cancellationToken);
        EnsurePending(verification);

        verification.Status = IdentityVerificationStatus.Rejected;
        verification.ReviewedAt = DateTime.UtcNow;
        verification.ReviewedByUserId = reviewerUserId;
        verification.RejectionReason = request.RejectionReason.Trim();
        verification.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(id, cancellationToken))!.ToDto();
    }

    private async Task<IdentityVerificationRequest> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Identity verification request not found.");

    private static void EnsurePending(IdentityVerificationRequest verification)
    {
        if (verification.Status is not IdentityVerificationStatus.Pending)
        {
            throw new ConflictException($"This request has already been {verification.Status}.");
        }
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
