using ShilpoHubBD.Application.DTOs.CustomOrders;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Application.Services.CustomOrders;

public class CustomOrderService : ICustomOrderService
{
    private readonly ICustomOrderRepository _customOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDeliveryTrackingService _deliveryTrackingService;

    public CustomOrderService(
        ICustomOrderRepository customOrderRepository, IUserRepository userRepository, IProductRepository productRepository,
        IDeliveryTrackingService deliveryTrackingService)
    {
        _customOrderRepository = customOrderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _deliveryTrackingService = deliveryTrackingService;
    }

    public async Task<CustomOrderRequestDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var request = await _customOrderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom order request not found.");

        if (!isAdmin && request.ProducerId != currentUserId && request.CustomerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this custom order request.");
        }

        return ToDto(request);
    }

    public async Task<List<CustomOrderRequestDto>> GetMineAsCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var requests = await _customOrderRepository.GetByCustomerAsync(customerId, cancellationToken);
        return requests.Select(ToDto).ToList();
    }

    public async Task<List<CustomOrderRequestDto>> GetMineAsProducerAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var requests = await _customOrderRepository.GetByProducerAsync(producerId, cancellationToken);
        return requests.Select(ToDto).ToList();
    }

    public async Task<CustomOrderRequestDto> CreateAsync(Guid customerId, CreateCustomOrderRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        if (!producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("The specified user is not a producer.");
        }

        if (request.ProductId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken)
                ?? throw new NotFoundException("Product not found.");

            if (product.ProducerId != request.ProducerId)
            {
                throw new ConflictException("The referenced product does not belong to this producer.");
            }
        }

        var now = DateTime.UtcNow;
        var customOrder = new CustomOrderRequest
        {
            Id = Guid.NewGuid(),
            ProducerId = request.ProducerId,
            CustomerId = customerId,
            ProductId = request.ProductId,
            Title = request.Title.Trim(),
            Specifications = request.Specifications.Trim(),
            Budget = request.Budget,
            Deadline = request.Deadline,
            RecipientName = request.RecipientName.Trim(),
            RecipientPhone = request.RecipientPhone.Trim(),
            ShippingAddressLine = request.ShippingAddressLine.Trim(),
            ShippingDistrictId = request.ShippingDistrictId,
            Status = CustomOrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _customOrderRepository.AddAsync(customOrder, cancellationToken);
        await _customOrderRepository.SaveChangesAsync(cancellationToken);

        var created = await _customOrderRepository.GetByIdAsync(customOrder.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<CustomOrderRequestDto> RespondAsync(Guid id, Guid producerId, bool isAdmin, RespondToCustomOrderRequest request, CancellationToken cancellationToken)
    {
        var customOrder = await _customOrderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom order request not found.");

        if (!isAdmin && customOrder.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to respond to this custom order request.");
        }

        if (customOrder.Status is CustomOrderStatus.Completed or CustomOrderStatus.Cancelled or CustomOrderStatus.Rejected
            or CustomOrderStatus.Shipped or CustomOrderStatus.Delivered)
        {
            throw new ConflictException("This custom order request is already closed and cannot be updated.");
        }

        // A producer answers a request; Pending and Cancelled are not answers (Cancelled is the customer's action).
        if (request.Status is CustomOrderStatus.Pending or CustomOrderStatus.Cancelled
            or CustomOrderStatus.Shipped or CustomOrderStatus.Delivered)
        {
            throw new ConflictException("A custom order can only be accepted, rejected, started or completed by the producer. Shipping and delivery go through the logistics partner.");
        }

        customOrder.Status = request.Status;
        customOrder.QuotedPrice = request.QuotedPrice;
        customOrder.ProducerResponse = string.IsNullOrWhiteSpace(request.ResponseMessage) ? null : request.ResponseMessage.Trim();
        customOrder.RespondedAt = DateTime.UtcNow;
        customOrder.UpdatedAt = DateTime.UtcNow;

        await _customOrderRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customOrder);
    }

    public async Task<CustomOrderRequestDto> UpdateDeliveryAsync(
        Guid id, Guid customerId, UpdateCustomOrderDeliveryRequest request, CancellationToken cancellationToken)
    {
        var customOrder = await _customOrderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom order request not found.");

        if (customOrder.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this custom order request.");
        }

        if (customOrder.Status is CustomOrderStatus.Shipped or CustomOrderStatus.Delivered
            or CustomOrderStatus.Cancelled or CustomOrderStatus.Rejected)
        {
            throw new ConflictException("The delivery address can no longer be changed for this custom order.");
        }

        customOrder.RecipientName = request.RecipientName.Trim();
        customOrder.RecipientPhone = request.RecipientPhone.Trim();
        customOrder.ShippingAddressLine = request.ShippingAddressLine.Trim();
        customOrder.ShippingDistrictId = request.ShippingDistrictId;
        customOrder.UpdatedAt = DateTime.UtcNow;

        await _customOrderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(customOrder);
    }

    // After a producer marks the piece Completed it goes to a logistics partner exactly like a normal
    // order item: the partner gets a shipment, the customer and producer follow the tracking, and only
    // the partner marking it delivered closes the order.
    public async Task<CustomOrderRequestDto> ShipAsync(
        Guid id, Guid producerId, bool isAdmin, ShipCustomOrderRequest request, CancellationToken cancellationToken)
    {
        var customOrder = await _customOrderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom order request not found.");

        if (!isAdmin && customOrder.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to ship this custom order request.");
        }

        if (customOrder.Status != CustomOrderStatus.Completed)
        {
            throw new ConflictException("Mark the custom order Completed before handing it to a logistics partner.");
        }

        if (string.IsNullOrWhiteSpace(customOrder.ShippingAddressLine) || string.IsNullOrWhiteSpace(customOrder.RecipientName))
        {
            throw new ConflictException("The customer has not provided a delivery address for this custom order yet.");
        }

        var shipment = await _deliveryTrackingService.CreateForOrderHandoffAsync(
            request.LogisticsPartnerProfileId, customOrder.ProducerId, customOrder.Producer.FullName,
            new OrderHandoffDetails
            {
                OrderId = null,
                RecipientName = customOrder.RecipientName!,
                RecipientPhone = customOrder.RecipientPhone ?? string.Empty,
                DestinationAddressLine = customOrder.ShippingAddressLine!,
                DestinationCity = "Bangladesh",
                DestinationDistrictId = customOrder.ShippingDistrictId,
                ParcelCount = 1,
                DeclaredValue = customOrder.QuotedPrice,
                Description = $"Custom order: {customOrder.Title}",
                DeliveryRouteId = request.DeliveryRouteId,
                WeightKg = request.WeightKg,
                Notes = request.Notes,
            },
            cancellationToken);

        var now = DateTime.UtcNow;
        customOrder.Status = CustomOrderStatus.Shipped;
        customOrder.TrackingNumber = shipment.TrackingNumber;
        customOrder.Carrier = shipment.LogisticsPartnerName ?? "Logistics partner";
        customOrder.ShippedAt = now;
        customOrder.UpdatedAt = now;

        await _customOrderRepository.SaveChangesAsync(cancellationToken);
        return ToDto(customOrder);
    }

    public async Task<CustomOrderRequestDto> CancelAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
    {
        var customOrder = await _customOrderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Custom order request not found.");

        if (customOrder.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to cancel this custom order request.");
        }

        if (customOrder.Status != CustomOrderStatus.Pending)
        {
            throw new ConflictException("Only pending custom order requests can be cancelled.");
        }

        customOrder.Status = CustomOrderStatus.Cancelled;
        customOrder.UpdatedAt = DateTime.UtcNow;

        await _customOrderRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customOrder);
    }

    private static CustomOrderRequestDto ToDto(CustomOrderRequest request) => new()
    {
        Id = request.Id,
        ProducerId = request.ProducerId,
        ProducerName = request.Producer.FullName,
        CustomerId = request.CustomerId,
        CustomerName = request.Customer.FullName,
        ProductId = request.ProductId,
        ProductName = request.Product?.Name,
        Title = request.Title,
        Specifications = request.Specifications,
        Budget = request.Budget,
        Deadline = request.Deadline,
        Status = request.Status,
        QuotedPrice = request.QuotedPrice,
        ProducerResponse = request.ProducerResponse,
        RespondedAt = request.RespondedAt,
        RecipientName = request.RecipientName,
        RecipientPhone = request.RecipientPhone,
        ShippingAddressLine = request.ShippingAddressLine,
        ShippingDistrictId = request.ShippingDistrictId,
        TrackingNumber = request.TrackingNumber,
        Carrier = request.Carrier,
        ShippedAt = request.ShippedAt,
        DeliveredAt = request.DeliveredAt,
        CreatedAt = request.CreatedAt,
        UpdatedAt = request.UpdatedAt,
    };
}
