using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Quotations;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.Services.Quotations;

public class QuotationService : IQuotationService
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public QuotationService(
        IQuotationRepository quotationRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _quotationRepository = quotationRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<QuotationRequestDto> CreateAsync(Guid businessPartnerId, CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetByIdAsync(businessPartnerId, cancellationToken) is null)
        {
            throw new NotFoundException("User not found.");
        }

        var now = DateTime.UtcNow;
        var quotation = new QuotationRequest
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim(),
            RequiredDeliveryDate = request.RequiredDeliveryDate,
            Status = QuotationRequestStatus.Sent,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var item in request.Items)
        {
            if (item.CategoryId.HasValue && await _categoryRepository.GetByIdAsync(item.CategoryId.Value, cancellationToken) is null)
            {
                throw new NotFoundException($"Category '{item.CategoryId}' not found.");
            }

            if (item.ProductId.HasValue && await _productRepository.GetByIdAsync(item.ProductId.Value, cancellationToken) is null)
            {
                throw new NotFoundException($"Product '{item.ProductId}' not found.");
            }

            quotation.Items.Add(new QuotationRequestItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = item.ProductName.Trim(),
                CategoryId = item.CategoryId,
                Quantity = item.Quantity,
                TargetPrice = item.TargetPrice,
                Specifications = string.IsNullOrWhiteSpace(item.Specifications) ? null : item.Specifications.Trim(),
            });
        }

        foreach (var producerId in request.ProducerIds.Distinct())
        {
            var producer = await _userRepository.GetByIdWithRolesAsync(producerId, cancellationToken);
            if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
            {
                throw new NotFoundException($"Producer '{producerId}' not found.");
            }

            quotation.Recipients.Add(new QuotationRequestProducer
            {
                Id = Guid.NewGuid(),
                ProducerId = producerId,
                Status = QuotationRecipientStatus.Invited,
                InvitedAt = now,
            });
        }

        quotation.StatusHistory.Add(new QuotationStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = QuotationRequestStatus.Sent,
            Note = $"Sent to {quotation.Recipients.Count} producer(s).",
            CreatedAt = now,
        });

        await _quotationRepository.AddAsync(quotation, cancellationToken);
        await _quotationRepository.SaveChangesAsync(cancellationToken);

        var created = await _quotationRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken)
            ?? throw new NotFoundException("Quotation request not found.");
        return ToDto(created);
    }

    public async Task<PagedResult<QuotationRequestListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, QuotationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _quotationRepository.GetPagedAllAsync(parameters, cancellationToken)
            : await _quotationRepository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<QuotationRequestListItemDto>> GetForProducerAsync(
        Guid producerId, QuotationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _quotationRepository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<QuotationRequestDto> GetByIdForBusinessPartnerAsync(
        Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Quotation request not found.");

        if (!isAdmin && quotation.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this quotation request.");
        }

        return ToDto(quotation);
    }

    public async Task<QuotationRequestDto> GetByIdForProducerAsync(Guid id, Guid producerId, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Quotation request not found.");

        if (!quotation.Recipients.Any(r => r.ProducerId == producerId))
        {
            throw new UnauthorizedAccessException("You were not invited to this quotation request.");
        }

        var dto = ToDto(quotation);
        // A recipient should only see their own response, not competing producers' bids.
        dto.Recipients = dto.Recipients.Where(r => r.ProducerId == producerId).ToList();
        return dto;
    }

    public async Task<List<QuotationResponseDto>> CompareAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Quotation request not found.");

        if (!isAdmin && quotation.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this quotation request.");
        }

        return quotation.Recipients
            .Where(r => r.Response is not null)
            .Select(r => ToResponseDto(r.Response!, r))
            .OrderBy(r => r.TotalPrice)
            .ToList();
    }

    public async Task<QuotationResponseDto> SubmitResponseAsync(
        Guid quotationRequestId, Guid producerId, SubmitQuotationResponseRequest request, CancellationToken cancellationToken)
    {
        var recipient = await _quotationRepository.GetRecipientAsync(quotationRequestId, producerId, cancellationToken)
            ?? throw new NotFoundException("You were not invited to this quotation request.");

        if (recipient.QuotationRequest.Status is QuotationRequestStatus.Cancelled or QuotationRequestStatus.Closed)
        {
            throw new ConflictException("This quotation request is no longer accepting responses.");
        }

        if (recipient.Response is not null)
        {
            throw new ConflictException("A response has already been submitted for this quotation request.");
        }

        var requestItemIds = recipient.QuotationRequest.Items.Select(i => i.Id).ToHashSet();
        foreach (var item in request.Items)
        {
            if (!requestItemIds.Contains(item.QuotationRequestItemId))
            {
                throw new NotFoundException($"Quotation request item '{item.QuotationRequestItemId}' not found on this request.");
            }
        }

        var now = DateTime.UtcNow;
        var response = new QuotationResponse
        {
            Id = Guid.NewGuid(),
            QuotationRequestProducerId = recipient.Id,
            TotalPrice = request.TotalPrice,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = QuotationResponseStatus.Submitted,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var item in request.Items)
        {
            var requestItem = recipient.QuotationRequest.Items.First(x => x.Id == item.QuotationRequestItemId);
            response.Items.Add(new QuotationResponseItem
            {
                Id = Guid.NewGuid(),
                QuotationRequestItemId = item.QuotationRequestItemId,
                QuotationRequestItem = requestItem,
                QuotedUnitPrice = item.QuotedUnitPrice,
                QuotedQuantity = item.QuotedQuantity,
                Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim(),
            });
        }

        recipient.Response = response;
        recipient.Status = QuotationRecipientStatus.Responded;
        recipient.RespondedAt = now;

        // Persist the response first so the progress count below (a fresh DB query) sees it,
        // rather than trying to reconcile in-memory and on-disk state in one pass.
        await _quotationRepository.SaveChangesAsync(cancellationToken);
        await UpdateOverallStatusAfterResponseAsync(recipient.QuotationRequest, now, cancellationToken);

        return ToResponseDto(response, recipient);
    }

    public async Task<QuotationResponseDto> DecideResponseAsync(
        Guid quotationRequestId, Guid responseId, Guid businessPartnerId, bool isAdmin, QuotationResponseDecisionRequest request, CancellationToken cancellationToken)
    {
        var response = await _quotationRepository.GetResponseByIdAsync(quotationRequestId, responseId, cancellationToken)
            ?? throw new NotFoundException("Quotation response not found.");

        var quotation = response.QuotationRequestProducer.QuotationRequest;
        if (!isAdmin && quotation.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to decide on this quotation response.");
        }

        if (response.Status != QuotationResponseStatus.Submitted)
        {
            throw new ConflictException("This response has already been decided.");
        }

        var now = DateTime.UtcNow;
        response.Status = request.Status;
        response.DecidedAt = now;
        response.DecisionNotes = string.IsNullOrWhiteSpace(request.DecisionNotes) ? null : request.DecisionNotes.Trim();
        response.UpdatedAt = now;

        if (request.Status == QuotationResponseStatus.Accepted)
        {
            quotation.Status = QuotationRequestStatus.Closed;
            quotation.UpdatedAt = now;
            await _quotationRepository.AddStatusEventAsync(new QuotationStatusEvent
            {
                Id = Guid.NewGuid(),
                QuotationRequestId = quotation.Id,
                Status = QuotationRequestStatus.Closed,
                Note = $"Accepted response from producer '{response.QuotationRequestProducer.ProducerId}'.",
                CreatedAt = now,
            }, cancellationToken);
        }

        await _quotationRepository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(response, response.QuotationRequestProducer);
    }

    public async Task<QuotationRequestDto> CancelAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Quotation request not found.");

        if (!isAdmin && quotation.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to cancel this quotation request.");
        }

        if (quotation.Status is QuotationRequestStatus.Closed or QuotationRequestStatus.Cancelled)
        {
            throw new ConflictException("This quotation request can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        quotation.Status = QuotationRequestStatus.Cancelled;
        quotation.UpdatedAt = now;
        quotation.StatusHistory.Add(new QuotationStatusEvent
        {
            Id = Guid.NewGuid(),
            QuotationRequestId = quotation.Id,
            Status = QuotationRequestStatus.Cancelled,
            CreatedAt = now,
        });

        await _quotationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(quotation);
    }

    private async Task UpdateOverallStatusAfterResponseAsync(QuotationRequest quotation, DateTime now, CancellationToken cancellationToken)
    {
        if (quotation.Status is not (QuotationRequestStatus.Sent or QuotationRequestStatus.PartiallyResponded))
        {
            return;
        }

        var (totalRecipients, respondedCount) = await _quotationRepository.GetRecipientProgressAsync(quotation.Id, cancellationToken);

        var newStatus = respondedCount >= totalRecipients && totalRecipients > 0
            ? QuotationRequestStatus.Responded
            : QuotationRequestStatus.PartiallyResponded;

        if (quotation.Status == newStatus)
        {
            return;
        }

        quotation.Status = newStatus;
        quotation.UpdatedAt = now;
        await _quotationRepository.AddStatusEventAsync(new QuotationStatusEvent
        {
            Id = Guid.NewGuid(),
            QuotationRequestId = quotation.Id,
            Status = newStatus,
            Note = $"{respondedCount} of {totalRecipients} producer(s) responded.",
            CreatedAt = now,
        }, cancellationToken);

        await _quotationRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string referenceNumber;

        do
        {
            referenceNumber = $"QUO-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _quotationRepository.ExistsByReferenceNumberAsync(referenceNumber, cancellationToken));

        return referenceNumber;
    }

    private static PagedResult<QuotationRequestListItemDto> ToPagedListDto(
        List<QuotationRequest> items, int totalCount, QuotationQueryParameters parameters)
    {
        return new PagedResult<QuotationRequestListItemDto>
        {
            Items = items.Select(q => new QuotationRequestListItemDto
            {
                Id = q.Id,
                ReferenceNumber = q.ReferenceNumber,
                Title = q.Title,
                RequiredDeliveryDate = q.RequiredDeliveryDate,
                Status = q.Status,
                ItemCount = q.Items.Count,
                RecipientCount = q.Recipients.Count,
                ResponseCount = q.Recipients.Count(r => r.Response is not null),
                CreatedAt = q.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static QuotationRequestDto ToDto(QuotationRequest quotation) => new()
    {
        Id = quotation.Id,
        ReferenceNumber = quotation.ReferenceNumber,
        BusinessPartnerId = quotation.BusinessPartnerId,
        BusinessPartnerName = quotation.BusinessPartner.FullName,
        Title = quotation.Title,
        Requirements = quotation.Requirements,
        RequiredDeliveryDate = quotation.RequiredDeliveryDate,
        Status = quotation.Status,
        Items = quotation.Items.Select(i => new QuotationRequestItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            CategoryId = i.CategoryId,
            CategoryName = i.Category?.Name,
            Quantity = i.Quantity,
            TargetPrice = i.TargetPrice,
            Specifications = i.Specifications,
        }).ToList(),
        Recipients = quotation.Recipients.Select(r => new QuotationRecipientDto
        {
            Id = r.Id,
            ProducerId = r.ProducerId,
            ProducerName = r.Producer.FullName,
            Status = r.Status,
            InvitedAt = r.InvitedAt,
            ViewedAt = r.ViewedAt,
            RespondedAt = r.RespondedAt,
            Response = r.Response is null ? null : ToResponseDto(r.Response, r),
        }).ToList(),
        StatusHistory = quotation.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new QuotationStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = quotation.CreatedAt,
        UpdatedAt = quotation.UpdatedAt,
    };

    private static QuotationResponseDto ToResponseDto(QuotationResponse response, QuotationRequestProducer recipient) => new()
    {
        Id = response.Id,
        QuotationRequestProducerId = recipient.Id,
        ProducerId = recipient.ProducerId,
        ProducerName = recipient.Producer?.FullName ?? string.Empty,
        TotalPrice = response.TotalPrice,
        EstimatedDeliveryDate = response.EstimatedDeliveryDate,
        Notes = response.Notes,
        Status = response.Status,
        DecidedAt = response.DecidedAt,
        DecisionNotes = response.DecisionNotes,
        Items = response.Items.Select(i => new QuotationResponseItemDto
        {
            Id = i.Id,
            QuotationRequestItemId = i.QuotationRequestItemId,
            ProductName = i.QuotationRequestItem?.ProductName ?? string.Empty,
            RequestedQuantity = i.QuotationRequestItem?.Quantity ?? 0,
            QuotedUnitPrice = i.QuotedUnitPrice,
            QuotedQuantity = i.QuotedQuantity,
            Notes = i.Notes,
        }).ToList(),
        CreatedAt = response.CreatedAt,
        UpdatedAt = response.UpdatedAt,
    };
}
