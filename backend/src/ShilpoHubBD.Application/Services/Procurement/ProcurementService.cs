using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Procurement;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Procurement;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.Services.Procurement;

public class ProcurementService : IProcurementService
{
    private readonly IProcurementRepository _procurementRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IBusinessPartnerRepository _businessPartnerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;

    // A bulk deal only goes ahead once the partner has paid at least half of it up front.
    private const decimal MinimumAdvanceShare = 0.5m;

    public ProcurementService(
        IProcurementRepository procurementRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IQuotationRepository quotationRepository,
        IBusinessPartnerRepository businessPartnerRepository,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
        _procurementRepository = procurementRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _quotationRepository = quotationRepository;
        _businessPartnerRepository = businessPartnerRepository;
        _orderRepository = orderRepository;
    }

    public async Task<ProcurementRequestDto> CreateAsync(Guid businessPartnerId, CreateProcurementRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var now = DateTime.UtcNow;
        var procurement = new ProcurementRequest
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ProducerId = request.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            Budget = request.Budget,
            DeliveryDeadline = request.DeliveryDeadline,
            Status = ProcurementStatus.PendingApproval,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException($"Product '{item.ProductId}' not found.");

            procurement.Items.Add(new ProcurementItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Specifications = string.IsNullOrWhiteSpace(item.Specifications) ? null : item.Specifications.Trim(),
            });
        }

        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProcurementStatus.PendingApproval,
            Note = "Procurement request created.",
            CreatedAt = now,
        });

        await _procurementRepository.AddAsync(procurement, cancellationToken);
        await _procurementRepository.SaveChangesAsync(cancellationToken);

        var created = await _procurementRepository.GetByIdWithDetailsAsync(procurement.Id, cancellationToken)
            ?? throw new NotFoundException("Procurement request not found.");
        return ToDto(created);
    }

    public async Task<ProcurementRequestDto> CreateFromQuotationResponseAsync(
        Guid businessPartnerId, bool isAdmin, Guid quotationResponseId, CreateProcurementFromQuotationRequest request, CancellationToken cancellationToken)
    {
        var response = await _quotationRepository.GetResponseByIdAsync(quotationResponseId, cancellationToken)
            ?? throw new NotFoundException("Quotation response not found.");

        var quotation = response.QuotationRequestProducer.QuotationRequest;
        if (!isAdmin && quotation.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to convert this quotation response.");
        }

        if (response.Status != QuotationResponseStatus.Accepted)
        {
            throw new ConflictException("Only an accepted quotation response can be converted into a procurement request.");
        }

        var deliveryDeadline = request.DeliveryDeadline ?? response.EstimatedDeliveryDate
            ?? throw new ConflictException("A delivery deadline is required; the quotation response did not include an estimated delivery date.");

        var now = DateTime.UtcNow;
        var procurement = new ProcurementRequest
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ProducerId = response.QuotationRequestProducer.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = string.IsNullOrWhiteSpace(request.Title) ? quotation.Title : request.Title.Trim(),
            Budget = request.Budget ?? response.TotalPrice,
            DeliveryDeadline = deliveryDeadline,
            Status = ProcurementStatus.PendingApproval,
            QuotationRequestId = quotation.Id,
            QuotationResponseId = response.Id,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var responseItem in response.Items)
        {
            // Quotation items may describe a requirement with no catalog product attached; only
            // items tied to a real Product can carry through into a procurement (and later an Order).
            var productId = responseItem.QuotationRequestItem.ProductId
                ?? throw new ConflictException($"Quotation item '{responseItem.QuotationRequestItem.ProductName}' is not linked to a catalog product and cannot be converted.");

            procurement.Items.Add(new ProcurementItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductName = responseItem.QuotationRequestItem.ProductName,
                Quantity = responseItem.QuotedQuantity,
                UnitPrice = responseItem.QuotedUnitPrice,
                Specifications = responseItem.Notes,
            });
        }

        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProcurementStatus.PendingApproval,
            Note = $"Converted from accepted quotation '{quotation.ReferenceNumber}'.",
            CreatedAt = now,
        });

        await _procurementRepository.AddAsync(procurement, cancellationToken);
        await _procurementRepository.SaveChangesAsync(cancellationToken);

        var created = await _procurementRepository.GetByIdWithDetailsAsync(procurement.Id, cancellationToken)
            ?? throw new NotFoundException("Procurement request not found.");
        return ToDto(created);
    }

    public async Task<PagedResult<ProcurementRequestListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, ProcurementQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _procurementRepository.GetPagedAllAsync(parameters, cancellationToken)
            : await _procurementRepository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return new PagedResult<ProcurementRequestListItemDto>
        {
            Items = items.Select(p => new ProcurementRequestListItemDto
            {
                Id = p.Id,
                ReferenceNumber = p.ReferenceNumber,
                Title = p.Title,
                ProducerName = p.Producer.FullName,
                ItemsTotal = p.Items.Sum(i => i.UnitPrice * i.Quantity),
                DeliveryDeadline = p.DeliveryDeadline,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    public async Task<ProcurementRequestDto> GetByIdAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var procurement = await GetOwnedAsync(id, businessPartnerId, isAdmin, cancellationToken);
        return ToDto(procurement);
    }

    public async Task<ProcurementRequestDto> ApproveAsync(
        Guid id, Guid userId, bool isAdmin, ProcurementDecisionRequest request, CancellationToken cancellationToken)
    {
        var procurement = await GetForProducerDecisionAsync(id, userId, isAdmin, cancellationToken);

        if (procurement.Status != ProcurementStatus.PendingApproval)
        {
            throw new ConflictException("Only a pending procurement request can be approved.");
        }

        var now = DateTime.UtcNow;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        procurement.Status = ProcurementStatus.Approved;
        procurement.ApprovedByUserId = userId;
        procurement.ApprovedAt = now;
        procurement.ApprovalNotes = notes;
        procurement.UpdatedAt = now;
        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProcurementStatus.Approved,
            Note = notes ?? "Accepted by the producer. The business partner now pays an advance of at least 50%.",
            CreatedAt = now,
        });

        await _procurementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(procurement);
    }

    public async Task<ProcurementRequestDto> RejectAsync(
        Guid id, Guid userId, bool isAdmin, ProcurementDecisionRequest request, CancellationToken cancellationToken)
    {
        var procurement = await GetForProducerDecisionAsync(id, userId, isAdmin, cancellationToken);

        if (procurement.Status != ProcurementStatus.PendingApproval)
        {
            throw new ConflictException("Only a pending procurement request can be rejected.");
        }

        var now = DateTime.UtcNow;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        procurement.Status = ProcurementStatus.Rejected;
        procurement.ApprovedByUserId = userId;
        procurement.ApprovedAt = now;
        procurement.ApprovalNotes = notes;
        procurement.UpdatedAt = now;
        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProcurementStatus.Rejected,
            Note = notes,
            CreatedAt = now,
        });

        await _procurementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(procurement);
    }

    public async Task<PagedResult<ProcurementRequestListItemDto>> GetForProducerAsync(
        Guid producerId, ProcurementQueryParameters parameters, CancellationToken cancellationToken)
    {
        NormalisePaging(parameters);
        var (items, total) = await _procurementRepository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPage(items, total, parameters);
    }

    public async Task<ProcurementRequestDto> PayAdvanceAsync(
        Guid id, Guid businessPartnerId, bool isAdmin, PayProcurementAdvanceRequest request, CancellationToken cancellationToken)
    {
        var procurement = await GetOwnedAsync(id, businessPartnerId, isAdmin, cancellationToken);

        if (procurement.Status != ProcurementStatus.Approved)
        {
            throw new ConflictException("The producer must accept the request before an advance can be paid.");
        }

        if (procurement.AdvancePaidAt.HasValue && procurement.AdvanceRefundedAt is null)
        {
            throw new ConflictException("The advance has already been paid for this request.");
        }

        var total = procurement.Items.Sum(i => i.UnitPrice * i.Quantity);
        var required = RequiredAdvance(total);
        if (request.Amount < required)
        {
            throw new ConflictException($"The advance must be at least 50% of the total: ৳{required:N0} or more.");
        }

        if (request.Amount > total)
        {
            throw new ConflictException($"The advance cannot be more than the total of ৳{total:N0}.");
        }

        var now = DateTime.UtcNow;
        procurement.AdvanceAmount = request.Amount;
        procurement.AdvancePaidAt = now;
        procurement.AdvanceRefundedAt = null;
        procurement.AdvanceMethod = string.IsNullOrWhiteSpace(request.Method) ? null : request.Method.Trim();
        procurement.AdvanceReference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim();
        procurement.InspectionStatus = ProcurementInspectionStatus.Pending;
        procurement.InspectedByUserId = null;
        procurement.InspectedAt = null;
        procurement.InspectionNotes = null;
        procurement.UpdatedAt = now;
        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = procurement.Status,
            Note = $"Advance of ৳{request.Amount:N0} paid ({request.Amount / total:P0} of the total). Waiting for the admin inspection.",
            CreatedAt = now,
        });

        await _procurementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(procurement);
    }

    public async Task<PagedResult<ProcurementRequestListItemDto>> GetForInspectionAsync(
        bool pendingOnly, ProcurementQueryParameters parameters, CancellationToken cancellationToken)
    {
        NormalisePaging(parameters);
        var (items, total) = await _procurementRepository.GetPagedForInspectionAsync(pendingOnly, parameters, cancellationToken);
        return ToPage(items, total, parameters);
    }

    public async Task<ProcurementRequestDto> InspectAsync(
        Guid id, Guid adminUserId, ProcurementInspectionRequest request, CancellationToken cancellationToken)
    {
        var procurement = await _procurementRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Procurement request not found.");

        if (procurement.InspectionStatus != ProcurementInspectionStatus.Pending)
        {
            throw new ConflictException("This deal is not waiting for an inspection.");
        }

        var now = DateTime.UtcNow;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        procurement.InspectedByUserId = adminUserId;
        procurement.InspectedAt = now;
        procurement.InspectionNotes = notes;
        procurement.UpdatedAt = now;

        if (request.Approve)
        {
            procurement.InspectionStatus = ProcurementInspectionStatus.Approved;
            procurement.StatusHistory.Add(new ProcurementStatusEvent
            {
                Id = Guid.NewGuid(),
                Status = procurement.Status,
                Note = notes ?? "Inspected and approved by the admin. The deal can now be converted to an order.",
                CreatedAt = now,
            });
        }
        else
        {
            procurement.InspectionStatus = ProcurementInspectionStatus.Rejected;
            procurement.Status = ProcurementStatus.Rejected;
            RefundAdvance(procurement, now, notes ?? "Rejected at inspection.");
        }

        await _procurementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(procurement);
    }

    public async Task<ProcurementRequestDto> ConvertToOrderAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var procurement = await GetOwnedAsync(id, userId, isAdmin, cancellationToken);

        if (procurement.Status != ProcurementStatus.Approved)
        {
            throw new ConflictException("Only an approved procurement request can be converted into an order.");
        }

        var totalDue = procurement.Items.Sum(i => i.UnitPrice * i.Quantity);
        if (procurement.AdvancePaidAt is null || procurement.AdvanceRefundedAt.HasValue
            || (procurement.AdvanceAmount ?? 0) < RequiredAdvance(totalDue))
        {
            throw new ConflictException("Pay the advance (at least 50% of the total) before converting this request into an order.");
        }

        if (procurement.InspectionStatus != ProcurementInspectionStatus.Approved)
        {
            throw new ConflictException("An admin must inspect and approve the deal before it can be converted into an order.");
        }

        var profile = await _businessPartnerRepository.GetByUserIdAsync(procurement.BusinessPartnerId, cancellationToken)
            ?? throw new ConflictException("Complete the business partner profile (for shipping/contact details) before converting this procurement request into an order.");

        if (profile.DistrictId is null)
        {
            throw new ConflictException("The business partner profile must have a district set before converting this procurement request into an order.");
        }

        var now = DateTime.UtcNow;
        var subtotal = procurement.Items.Sum(i => i.UnitPrice * i.Quantity);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = await GenerateUniqueOrderNumberAsync(cancellationToken),
            UserId = procurement.BusinessPartnerId,
            Status = OrderStatus.Pending,
            PaymentMethod = PaymentMethod.CashOnDelivery,
            Subtotal = subtotal,
            Total = subtotal,
            RecipientName = profile.ContactPersonName,
            RecipientPhone = profile.ContactPhone,
            ShippingAddressLine = profile.AddressLine,
            ShippingDistrictId = profile.DistrictId.Value,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var item in procurement.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.UnitPrice * item.Quantity,
            });
        }

        order.StatusHistory.Add(new OrderStatusEvent
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = OrderStatus.Pending,
            Note = $"Created from bulk procurement request '{procurement.ReferenceNumber}'.",
            CreatedAt = now,
        });

        await _orderRepository.AddAsync(order, cancellationToken);

        // The advance becomes a paid payment on the order, so a later return refunds it automatically.
        await _paymentRepository.AddAsync(new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Provider = PaymentMethod.CashOnDelivery.ToString(),
            Amount = procurement.AdvanceAmount!.Value,
            Status = PaymentStatus.Paid,
            TransactionReference = procurement.AdvanceReference ?? procurement.ReferenceNumber,
            PaidAt = procurement.AdvancePaidAt,
            CreatedAt = now,
            UpdatedAt = now,
        }, cancellationToken);

        procurement.Status = ProcurementStatus.Converted;
        procurement.OrderId = order.Id;
        procurement.UpdatedAt = now;
        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProcurementStatus.Converted,
            Note = $"Converted to order '{order.OrderNumber}'.",
            CreatedAt = now,
        });

        // Both the new Order and the ProcurementRequest mutations are tracked on the same
        // scoped DbContext, so this single SaveChanges commits them together.
        await _procurementRepository.SaveChangesAsync(cancellationToken);

        var updated = await _procurementRepository.GetByIdWithDetailsAsync(procurement.Id, cancellationToken)
            ?? throw new NotFoundException("Procurement request not found.");
        return ToDto(updated);
    }

    public async Task<ProcurementRequestDto> CancelAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var procurement = await GetOwnedAsync(id, userId, isAdmin, cancellationToken);

        if (procurement.Status is not (ProcurementStatus.PendingApproval or ProcurementStatus.Approved))
        {
            throw new ConflictException("This procurement request can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        procurement.Status = ProcurementStatus.Cancelled;
        procurement.UpdatedAt = now;
        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProcurementStatus.Cancelled,
            CreatedAt = now,
        });
        RefundAdvance(procurement, now, "Cancelled before conversion.");

        await _procurementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(procurement);
    }

    private static decimal RequiredAdvance(decimal total) => Math.Ceiling(total * MinimumAdvanceShare);

    // An advance that was paid but never used (deal cancelled or failed inspection) goes back to the partner.
    private static void RefundAdvance(ProcurementRequest procurement, DateTime now, string reason)
    {
        if (procurement.AdvancePaidAt is null || procurement.AdvanceRefundedAt.HasValue)
        {
            return;
        }

        procurement.AdvanceRefundedAt = now;
        procurement.StatusHistory.Add(new ProcurementStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = procurement.Status,
            Note = $"Advance of ৳{procurement.AdvanceAmount:N0} refunded to the business partner. {reason}",
            CreatedAt = now,
        });
    }

    private static void NormalisePaging(ProcurementQueryParameters parameters)
    {
        parameters.Page = parameters.Page < 1 ? 1 : parameters.Page;
        parameters.PageSize = parameters.PageSize is < 1 or > 50 ? 20 : parameters.PageSize;
    }

    private static PagedResult<ProcurementRequestListItemDto> ToPage(
        List<ProcurementRequest> items, int total, ProcurementQueryParameters parameters) => new()
    {
        Items = items.Select(ToListItem).ToList(),
        TotalCount = total,
        Page = parameters.Page,
        PageSize = parameters.PageSize,
    };

    private static ProcurementRequestListItemDto ToListItem(ProcurementRequest p)
    {
        var total = p.Items.Sum(i => i.UnitPrice * i.Quantity);
        return new ProcurementRequestListItemDto
        {
            Id = p.Id,
            ReferenceNumber = p.ReferenceNumber,
            Title = p.Title,
            ProducerName = p.Producer.FullName,
            BusinessPartnerName = p.BusinessPartner?.FullName ?? string.Empty,
            ItemsTotal = total,
            RequiredAdvance = RequiredAdvance(total),
            AdvanceAmount = p.AdvanceAmount,
            AdvancePaidAt = p.AdvancePaidAt,
            AdvanceRefundedAt = p.AdvanceRefundedAt,
            InspectionStatus = p.InspectionStatus,
            InspectionNotes = p.InspectionNotes,
            DeliveryDeadline = p.DeliveryDeadline,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
        };
    }

    private async Task<ProcurementRequest> GetForProducerDecisionAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var procurement = await _procurementRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Procurement request not found.");

        if (!isAdmin && procurement.ProducerId != userId)
        {
            throw new UnauthorizedAccessException("Only the selected producer can accept or decline this request.");
        }

        return procurement;
    }

    private async Task<ProcurementRequest> GetOwnedAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var procurement = await _procurementRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Procurement request not found.");

        if (!isAdmin && procurement.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this procurement request.");
        }

        return procurement;
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string referenceNumber;

        do
        {
            referenceNumber = $"PROC-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _procurementRepository.ExistsByReferenceNumberAsync(referenceNumber, cancellationToken));

        return referenceNumber;
    }

    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken cancellationToken)
    {
        string orderNumber;

        do
        {
            orderNumber = $"SH-ORD-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _orderRepository.ExistsByOrderNumberAsync(orderNumber, cancellationToken));

        return orderNumber;
    }

    private static ProcurementRequestDto ToDto(ProcurementRequest procurement) => new()
    {
        Id = procurement.Id,
        ReferenceNumber = procurement.ReferenceNumber,
        BusinessPartnerId = procurement.BusinessPartnerId,
        BusinessPartnerName = procurement.BusinessPartner.FullName,
        ProducerId = procurement.ProducerId,
        ProducerName = procurement.Producer.FullName,
        Title = procurement.Title,
        Budget = procurement.Budget,
        ItemsTotal = procurement.Items.Sum(i => i.UnitPrice * i.Quantity),
        DeliveryDeadline = procurement.DeliveryDeadline,
        Status = procurement.Status,
        QuotationRequestId = procurement.QuotationRequestId,
        QuotationResponseId = procurement.QuotationResponseId,
        OrderId = procurement.OrderId,
        OrderNumber = procurement.Order?.OrderNumber,
        ApprovedByName = procurement.ApprovedBy?.FullName,
        ApprovedAt = procurement.ApprovedAt,
        ApprovalNotes = procurement.ApprovalNotes,
        RequiredAdvance = RequiredAdvance(procurement.Items.Sum(i => i.UnitPrice * i.Quantity)),
        AdvanceAmount = procurement.AdvanceAmount,
        AdvancePaidAt = procurement.AdvancePaidAt,
        AdvanceMethod = procurement.AdvanceMethod,
        AdvanceReference = procurement.AdvanceReference,
        AdvanceRefundedAt = procurement.AdvanceRefundedAt,
        InspectionStatus = procurement.InspectionStatus,
        InspectedByName = procurement.InspectedBy?.FullName,
        InspectedAt = procurement.InspectedAt,
        InspectionNotes = procurement.InspectionNotes,
        Items = procurement.Items.Select(i => new ProcurementItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Specifications = i.Specifications,
        }).ToList(),
        StatusHistory = procurement.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new ProcurementStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = procurement.CreatedAt,
        UpdatedAt = procurement.UpdatedAt,
    };
}
