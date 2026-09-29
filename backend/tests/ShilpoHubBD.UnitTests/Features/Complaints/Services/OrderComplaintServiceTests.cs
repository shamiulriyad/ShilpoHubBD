using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Complaints;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Complaints.Services;

[Trait("Feature", "Complaints")]
[Trait("Layer", "Service")]
public class OrderComplaintServiceTests
{
    private readonly IOrderComplaintRepository _repository = Substitute.For<IOrderComplaintRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly OrderComplaintService _service;

    public OrderComplaintServiceTests()
    {
        _service = new OrderComplaintService(_repository, _orderRepository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Order MakeOrder(Guid userId, OrderStatus status, Guid productId, Guid producerId)
    {
        var product = new Product { Id = productId, Name = "Nakshi Kantha", ProducerId = producerId };
        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-1",
            UserId = userId,
            Status = status,
            Items = new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = productId, Product = product },
            },
        };
    }

    private static OrderComplaint MakeComplaint(Guid customerId, Guid producerId, OrderComplaintStatus status)
    {
        var customer = TestUsers.Create(fullName: "Customer");
        var producer = TestUsers.Create(fullName: "Producer");
        producer.Id = producerId;
        customer.Id = customerId;
        var product = new Product { Id = Guid.NewGuid(), Name = "Nakshi Kantha", ProducerId = producerId, Producer = producer };
        var order = new Order { Id = Guid.NewGuid(), OrderNumber = "ORD-1", UserId = customerId };
        return new OrderComplaint
        {
            Id = Guid.NewGuid(),
            Order = order,
            OrderId = order.Id,
            Product = product,
            ProductId = product.Id,
            Producer = producer,
            ProducerId = producerId,
            Customer = customer,
            CustomerId = customerId,
            Subject = "Broken item",
            Description = "It arrived cracked.",
            Status = status,
        };
    }

    [Fact]
    public async Task CreateAsync_UnknownOrder_ThrowsNotFound()
    {
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(Guid.NewGuid(), new CreateOrderComplaintRequest { OrderId = Guid.NewGuid() }, Ct));
    }

    [Fact]
    public async Task CreateAsync_NotTheCustomersOrder_ThrowsUnauthorized()
    {
        var order = MakeOrder(Guid.NewGuid(), OrderStatus.Delivered, Guid.NewGuid(), Guid.NewGuid());
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.CreateAsync(Guid.NewGuid(), new CreateOrderComplaintRequest { OrderId = order.Id }, Ct));
    }

    [Fact]
    public async Task CreateAsync_OrderNotYetDelivered_ThrowsConflict()
    {
        var customerId = Guid.NewGuid();
        var order = MakeOrder(customerId, OrderStatus.Shipped, Guid.NewGuid(), Guid.NewGuid());
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(customerId, new CreateOrderComplaintRequest { OrderId = order.Id }, Ct));
    }

    [Fact]
    public async Task CreateAsync_ProductNotPartOfOrder_ThrowsConflict()
    {
        var customerId = Guid.NewGuid();
        var order = MakeOrder(customerId, OrderStatus.Delivered, Guid.NewGuid(), Guid.NewGuid());
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(customerId, new CreateOrderComplaintRequest { OrderId = order.Id, ProductId = Guid.NewGuid() }, Ct));
    }

    [Fact]
    public async Task CreateAsync_AlreadyHasAnOpenComplaintForThisItem_ThrowsConflict()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = MakeOrder(customerId, OrderStatus.Delivered, productId, Guid.NewGuid());
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _repository.HasOpenForOrderProductAsync(order.Id, productId, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(customerId, new CreateOrderComplaintRequest { OrderId = order.Id, ProductId = productId }, Ct));
    }

    [Fact]
    public async Task CreateAsync_Valid_SavesAndReturnsTheComplaint()
    {
        var customerId = Guid.NewGuid();
        var producerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = MakeOrder(customerId, OrderStatus.Delivered, productId, producerId);
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _repository.HasOpenForOrderProductAsync(order.Id, productId, Arg.Any<CancellationToken>()).Returns(false);

        var saved = MakeComplaint(customerId, producerId, OrderComplaintStatus.Open);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(saved);

        var request = new CreateOrderComplaintRequest
        {
            OrderId = order.Id,
            ProductId = productId,
            Subject = "  Broken item  ",
            Description = "  It arrived cracked.  ",
        };

        var result = await _service.CreateAsync(customerId, request, Ct);

        await _repository.Received(1).AddAsync(Arg.Is<OrderComplaint>(c =>
            c.OrderId == order.Id && c.ProductId == productId && c.ProducerId == producerId
            && c.CustomerId == customerId && c.Subject == "Broken item" && c.Description == "It arrived cracked."
            && c.Status == OrderComplaintStatus.Open), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
        Assert.Equal(saved.Id, result.Id);
        Assert.False(result.CanRate);
    }

    [Fact]
    public async Task CreateAsync_BlankImageUrl_IsStoredAsNull()
    {
        var customerId = Guid.NewGuid();
        var producerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = MakeOrder(customerId, OrderStatus.Delivered, productId, producerId);
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(MakeComplaint(customerId, producerId, OrderComplaintStatus.Open));

        var request = new CreateOrderComplaintRequest { OrderId = order.Id, ProductId = productId, ImageUrl = "   " };

        await _service.CreateAsync(customerId, request, Ct);

        await _repository.Received(1).AddAsync(Arg.Is<OrderComplaint>(c => c.ImageUrl == null), Ct);
    }

    [Fact]
    public async Task GetMineAsCustomerAsync_ReturnsTheCustomersComplaints()
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), OrderComplaintStatus.Open);
        _repository.GetByCustomerAsync(customerId, Arg.Any<CancellationToken>()).Returns(new List<OrderComplaint> { complaint });

        var result = await _service.GetMineAsCustomerAsync(customerId, Ct);

        Assert.Equal(complaint.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetMineAsProducerAsync_ReturnsTheProducersComplaints()
    {
        var producerId = Guid.NewGuid();
        var complaint = MakeComplaint(Guid.NewGuid(), producerId, OrderComplaintStatus.Open);
        _repository.GetByProducerAsync(producerId, Arg.Any<CancellationToken>()).Returns(new List<OrderComplaint> { complaint });

        var result = await _service.GetMineAsProducerAsync(producerId, Ct);

        Assert.Equal(complaint.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task RespondAsync_UnknownComplaint_ThrowsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((OrderComplaint?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.RespondAsync(Guid.NewGuid(), Guid.NewGuid(), new RespondToOrderComplaintRequest { Message = "x" }, Ct));
    }

    [Fact]
    public async Task RespondAsync_AboutAnotherProducer_ThrowsUnauthorized()
    {
        var complaint = MakeComplaint(Guid.NewGuid(), Guid.NewGuid(), OrderComplaintStatus.Open);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.RespondAsync(complaint.Id, Guid.NewGuid(), new RespondToOrderComplaintRequest { Message = "x" }, Ct));
    }

    [Fact]
    public async Task RespondAsync_NotOpen_ThrowsConflict()
    {
        var producerId = Guid.NewGuid();
        var complaint = MakeComplaint(Guid.NewGuid(), producerId, OrderComplaintStatus.Resolved);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RespondAsync(complaint.Id, producerId, new RespondToOrderComplaintRequest { Message = "x" }, Ct));
    }

    [Fact]
    public async Task RespondAsync_Valid_ResolvesAndRecordsTheResponse()
    {
        var producerId = Guid.NewGuid();
        var complaint = MakeComplaint(Guid.NewGuid(), producerId, OrderComplaintStatus.Open);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        var result = await _service.RespondAsync(complaint.Id, producerId, new RespondToOrderComplaintRequest { Message = "  Sent a replacement.  " }, Ct);

        Assert.Equal("Resolved", result.Status);
        Assert.Equal("Sent a replacement.", result.ProducerResponse);
        Assert.NotNull(result.RespondedAt);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task ConfirmSatisfiedAsync_NotTheCustomer_ThrowsUnauthorized()
    {
        var complaint = MakeComplaint(Guid.NewGuid(), Guid.NewGuid(), OrderComplaintStatus.Resolved);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.ConfirmSatisfiedAsync(complaint.Id, Guid.NewGuid(), new CustomerComplaintNoteRequest(), Ct));
    }

    [Fact]
    public async Task ConfirmSatisfiedAsync_NotYetResolved_ThrowsConflict()
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), OrderComplaintStatus.Open);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.ConfirmSatisfiedAsync(complaint.Id, customerId, new CustomerComplaintNoteRequest(), Ct));
    }

    [Fact]
    public async Task ConfirmSatisfiedAsync_Valid_MarksSatisfiedSoTheCustomerCanNowRate()
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), OrderComplaintStatus.Resolved);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        var result = await _service.ConfirmSatisfiedAsync(complaint.Id, customerId, new CustomerComplaintNoteRequest { Note = "  Thanks!  " }, Ct);

        Assert.Equal("Satisfied", result.Status);
        Assert.True(result.CanRate);
        Assert.Equal("Thanks!", result.CustomerNote);
    }

    [Fact]
    public async Task ConfirmSatisfiedAsync_BlankNote_IsStoredAsNull()
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), OrderComplaintStatus.Resolved);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        var result = await _service.ConfirmSatisfiedAsync(complaint.Id, customerId, new CustomerComplaintNoteRequest { Note = "   " }, Ct);

        Assert.Null(result.CustomerNote);
    }

    [Fact]
    public async Task ReopenAsync_NotResolved_ThrowsConflict()
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), OrderComplaintStatus.Open);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.ReopenAsync(complaint.Id, customerId, new CustomerComplaintNoteRequest(), Ct));
    }

    [Fact]
    public async Task ReopenAsync_Valid_SetsItBackToOpen()
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), OrderComplaintStatus.Resolved);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        var result = await _service.ReopenAsync(complaint.Id, customerId, new CustomerComplaintNoteRequest { Note = "Still broken" }, Ct);

        Assert.Equal("Open", result.Status);
        Assert.Equal("Still broken", result.CustomerNote);
    }

    [Theory]
    [InlineData(OrderComplaintStatus.Satisfied)]
    [InlineData(OrderComplaintStatus.Withdrawn)]
    public async Task WithdrawAsync_AlreadyClosed_ThrowsConflict(OrderComplaintStatus status)
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), status);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        await Assert.ThrowsAsync<ConflictException>(() => _service.WithdrawAsync(complaint.Id, customerId, Ct));
    }

    [Theory]
    [InlineData(OrderComplaintStatus.Open)]
    [InlineData(OrderComplaintStatus.Resolved)]
    public async Task WithdrawAsync_OpenOrResolved_SetsWithdrawn(OrderComplaintStatus status)
    {
        var customerId = Guid.NewGuid();
        var complaint = MakeComplaint(customerId, Guid.NewGuid(), status);
        _repository.GetByIdAsync(complaint.Id, Arg.Any<CancellationToken>()).Returns(complaint);

        var result = await _service.WithdrawAsync(complaint.Id, customerId, Ct);

        Assert.Equal("Withdrawn", result.Status);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }
}
