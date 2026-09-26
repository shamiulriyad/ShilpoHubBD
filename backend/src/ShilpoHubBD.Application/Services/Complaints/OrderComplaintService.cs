using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.Complaints;

// A customer who has a problem with something they received files a complaint, the producer answers it,
// and only when the customer confirms they are satisfied can they rate the producer and the product.
public class OrderComplaintService : IOrderComplaintService
{
    private readonly IOrderComplaintRepository _repository;
    private readonly IOrderRepository _orderRepository;

    public OrderComplaintService(IOrderComplaintRepository repository, IOrderRepository orderRepository)
    {
        _repository = repository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderComplaintDto> CreateAsync(Guid customerId, CreateOrderComplaintRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (order.UserId != customerId)
        {
            throw new UnauthorizedAccessException("This is not your order.");
        }

        if (order.Status != OrderStatus.Delivered)
        {
            throw new ConflictException("You can file a complaint once the order has been delivered.");
        }

        var item = order.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
            ?? throw new ConflictException("That product is not part of this order.");

        if (await _repository.HasOpenForOrderProductAsync(order.Id, item.ProductId, cancellationToken))
        {
            throw new ConflictException("You already have an open complaint about this item.");
        }

        var now = DateTime.UtcNow;
        var complaint = new OrderComplaint
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = item.ProductId,
            ProducerId = item.Product.ProducerId,
            CustomerId = customerId,
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            Status = OrderComplaintStatus.Open,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(complaint, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto((await _repository.GetByIdAsync(complaint.Id, cancellationToken))!);
    }

    public async Task<List<OrderComplaintDto>> GetMineAsCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        => (await _repository.GetByCustomerAsync(customerId, cancellationToken)).Select(ToDto).ToList();

    public async Task<List<OrderComplaintDto>> GetMineAsProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => (await _repository.GetByProducerAsync(producerId, cancellationToken)).Select(ToDto).ToList();

    public async Task<OrderComplaintDto> RespondAsync(Guid id, Guid producerId, RespondToOrderComplaintRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        if (complaint.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("This complaint is about another producer.");
        }

        if (complaint.Status != OrderComplaintStatus.Open)
        {
            throw new ConflictException("Only an open complaint can be answered.");
        }

        var now = DateTime.UtcNow;
        complaint.Status = OrderComplaintStatus.Resolved;
        complaint.ProducerResponse = request.Message.Trim();
        complaint.RespondedAt = now;
        complaint.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(complaint);
    }

    public async Task<OrderComplaintDto> ConfirmSatisfiedAsync(Guid id, Guid customerId, CustomerComplaintNoteRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadForCustomerAsync(id, customerId, cancellationToken);
        if (complaint.Status != OrderComplaintStatus.Resolved)
        {
            throw new ConflictException("The producer has not resolved this complaint yet.");
        }

        complaint.Status = OrderComplaintStatus.Satisfied;
        complaint.CustomerNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        complaint.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(complaint);
    }

    public async Task<OrderComplaintDto> ReopenAsync(Guid id, Guid customerId, CustomerComplaintNoteRequest request, CancellationToken cancellationToken)
    {
        var complaint = await LoadForCustomerAsync(id, customerId, cancellationToken);
        if (complaint.Status != OrderComplaintStatus.Resolved)
        {
            throw new ConflictException("Only a complaint the producer marked resolved can be reopened.");
        }

        complaint.Status = OrderComplaintStatus.Open;
        complaint.CustomerNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        complaint.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(complaint);
    }

    public async Task<OrderComplaintDto> WithdrawAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
    {
        var complaint = await LoadForCustomerAsync(id, customerId, cancellationToken);
        if (complaint.Status is not (OrderComplaintStatus.Open or OrderComplaintStatus.Resolved))
        {
            throw new ConflictException("This complaint is already closed.");
        }

        complaint.Status = OrderComplaintStatus.Withdrawn;
        complaint.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(complaint);
    }

    private async Task<OrderComplaint> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Complaint not found.");

    private async Task<OrderComplaint> LoadForCustomerAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
    {
        var complaint = await LoadAsync(id, cancellationToken);
        if (complaint.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("This is not your complaint.");
        }

        return complaint;
    }

    private static OrderComplaintDto ToDto(OrderComplaint c) => new()
    {
        Id = c.Id,
        OrderId = c.OrderId,
        OrderNumber = c.Order.OrderNumber,
        ProductId = c.ProductId,
        ProductName = c.Product.Name,
        ProducerId = c.ProducerId,
        ProducerName = c.Producer.FullName,
        CustomerId = c.CustomerId,
        CustomerName = c.Customer.FullName,
        Subject = c.Subject,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        Status = c.Status.ToString(),
        ProducerResponse = c.ProducerResponse,
        RespondedAt = c.RespondedAt,
        CustomerNote = c.CustomerNote,
        CanRate = c.Status == OrderComplaintStatus.Satisfied,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}
