using ShilpoHubBD.Application.DTOs.CustomOrders;
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

    public CustomOrderService(
        ICustomOrderRepository customOrderRepository, IUserRepository userRepository, IProductRepository productRepository)
    {
        _customOrderRepository = customOrderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
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

        if (customOrder.Status is CustomOrderStatus.Completed or CustomOrderStatus.Cancelled or CustomOrderStatus.Rejected)
        {
            throw new ConflictException("This custom order request is already closed and cannot be updated.");
        }

        customOrder.Status = request.Status;
        customOrder.QuotedPrice = request.QuotedPrice;
        customOrder.ProducerResponse = string.IsNullOrWhiteSpace(request.ResponseMessage) ? null : request.ResponseMessage.Trim();
        customOrder.RespondedAt = DateTime.UtcNow;
        customOrder.UpdatedAt = DateTime.UtcNow;

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
        CreatedAt = request.CreatedAt,
        UpdatedAt = request.UpdatedAt,
    };
}
