using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using SupplierDiscoveryService = ShilpoHubBD.Application.Services.SupplierDiscovery.SupplierDiscoveryService;

namespace ShilpoHubBD.UnitTests.Features.SupplierDiscovery.Services;

[Trait("Feature", "SupplierDiscovery")]
[Trait("Layer", "Service")]
public class SupplierDiscoveryServiceTests
{
    private readonly ISupplierDiscoveryRepository _repository = Substitute.For<ISupplierDiscoveryRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProducerOrderService _producerOrderService = Substitute.For<IProducerOrderService>();
    private readonly SupplierDiscoveryService _service;

    public SupplierDiscoveryServiceTests()
    {
        _service = new SupplierDiscoveryService(_repository, _userRepository, _productRepository, _producerOrderService);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SearchAsync_ReturnsAPagedResultUsingTheGivenPageAndPageSize()
    {
        var items = new List<SupplierSearchResultDto> { new() { ProducerId = Guid.NewGuid() } };
        var parameters = new SupplierSearchParameters { Page = 2, PageSize = 10 };
        _repository.SearchAsync(parameters, Arg.Any<CancellationToken>()).Returns((items, 21));

        var result = await _service.SearchAsync(parameters, Ct);

        Assert.Same(items, result.Items);
        Assert.Equal(21, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetProducerProfileAsync_UnknownUser_ThrowsNotFound()
    {
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ShilpoHubBD.Domain.Entities.Identity.User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProducerProfileAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetProducerProfileAsync_UserWithoutProducerRole_ThrowsNotFound()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.SuperAdmin);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProducerProfileAsync(user.Id, Ct));
    }

    [Fact]
    public async Task GetProducerProfileAsync_ProducerRoleButNoProfileRow_ThrowsNotFound()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetProducerProfileAsync(user.Id, Arg.Any<CancellationToken>()).Returns((SupplierProfileDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProducerProfileAsync(user.Id, Ct));
    }

    [Fact]
    public async Task GetProducerProfileAsync_Valid_ReturnsTheProfile()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        var profile = new SupplierProfileDto { ProducerId = user.Id, ProducerName = "Producer" };
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetProducerProfileAsync(user.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await _service.GetProducerProfileAsync(user.Id, Ct);

        Assert.Same(profile, result);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_UnknownUser_ThrowsNotFound()
    {
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ShilpoHubBD.Domain.Entities.Identity.User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBusinessProfileAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetBusinessProfileAsync_UserWithoutProducerRole_ThrowsNotFound()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.SuperAdmin);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBusinessProfileAsync(user.Id, Ct));
    }

    [Fact]
    public async Task GetBusinessProfileAsync_NoCraftProfileRow_ThrowsNotFound()
    {
        var user = TestUsers.Create().WithRoles(RoleNames.Producer);
        _userRepository.GetByIdWithRolesAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetProducerProfileAsync(user.Id, Arg.Any<CancellationToken>()).Returns((SupplierProfileDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBusinessProfileAsync(user.Id, Ct));
    }

    private void StubHappyPathDependencies(
        ShilpoHubBD.Domain.Entities.Identity.User producer, SupplierProfileDto craftProfile,
        RevenueDashboardDto overall, RevenueDashboardDto current, RevenueDashboardDto previous,
        List<ProducerCustomerDto>? customers = null)
    {
        _userRepository.GetByIdWithRolesAsync(producer.Id, Arg.Any<CancellationToken>()).Returns(producer);
        _repository.GetProducerProfileAsync(producer.Id, Arg.Any<CancellationToken>()).Returns(craftProfile);
        _productRepository.GetByProducerAsync(producer.Id, Arg.Any<CancellationToken>()).Returns(new List<Product>());
        _producerOrderService.GetRevenueDashboardAsync(producer.Id, Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var from = callInfo.ArgAt<DateTime?>(1);
                if (from is null)
                {
                    return overall;
                }

                var to = callInfo.ArgAt<DateTime?>(2)!.Value;
                return to > DateTime.UtcNow.AddDays(-15) ? current : previous;
            });
        _producerOrderService.GetSalesAnalyticsAsync(producer.Id, null, null, 5, Arg.Any<CancellationToken>())
            .Returns(new SalesAnalyticsDto());
        _producerOrderService.GetCustomersAsync(producer.Id, Arg.Any<CancellationToken>()).Returns(customers ?? new List<ProducerCustomerDto>());
    }

    [Fact]
    public async Task GetBusinessProfileAsync_Valid_MapsIdentityAndOverallRevenueFields()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto
        {
            ProducerId = producer.Id, ProducerName = "Nakshi House", WorkshopName = "Old Dhaka Workshop",
            PrimaryCraft = "Weaving", DistrictName = "Dhaka", AverageRating = 4.5m, TotalReviewCount = 12,
        };
        var overall = new RevenueDashboardDto { TotalRevenue = 5000, TotalOrders = 20, TotalItemsSold = 30, AverageOrderValue = 250 };
        StubHappyPathDependencies(producer, craftProfile, overall, new RevenueDashboardDto(), new RevenueDashboardDto());

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Equal("Nakshi House", result.ProducerName);
        Assert.Equal("Old Dhaka Workshop", result.WorkshopName);
        Assert.Equal(5000, result.TotalRevenue);
        Assert.Equal(20, result.TotalOrders);
        Assert.Equal(4.5m, result.AverageRating);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_PreviousRevenueZeroAndCurrentPositive_SalesGrowthIsOneHundred()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto { ProducerId = producer.Id, ProducerName = "x" };
        StubHappyPathDependencies(producer, craftProfile,
            overall: new RevenueDashboardDto(), current: new RevenueDashboardDto { TotalRevenue = 1000 }, previous: new RevenueDashboardDto { TotalRevenue = 0 });

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Equal(100m, result.SalesGrowthPercent);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_BothPeriodsZero_SalesGrowthIsNull()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto { ProducerId = producer.Id, ProducerName = "x" };
        StubHappyPathDependencies(producer, craftProfile,
            overall: new RevenueDashboardDto(), current: new RevenueDashboardDto { TotalRevenue = 0 }, previous: new RevenueDashboardDto { TotalRevenue = 0 });

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Null(result.SalesGrowthPercent);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_RevenueGrewFromAPositivePreviousPeriod_ComputesPercentChange()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto { ProducerId = producer.Id, ProducerName = "x" };
        StubHappyPathDependencies(producer, craftProfile,
            overall: new RevenueDashboardDto(), current: new RevenueDashboardDto { TotalRevenue = 1500 }, previous: new RevenueDashboardDto { TotalRevenue = 1000 });

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Equal(50m, result.SalesGrowthPercent);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_RepeatCustomerRate_CountsOnlyCustomersWithMoreThanOneOrder()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto { ProducerId = producer.Id, ProducerName = "x" };
        var customers = new List<ProducerCustomerDto>
        {
            new() { CustomerId = Guid.NewGuid(), TotalOrders = 3 },
            new() { CustomerId = Guid.NewGuid(), TotalOrders = 1 },
            new() { CustomerId = Guid.NewGuid(), TotalOrders = 1 },
            new() { CustomerId = Guid.NewGuid(), TotalOrders = 2 },
        };
        StubHappyPathDependencies(producer, craftProfile, new RevenueDashboardDto(), new RevenueDashboardDto(), new RevenueDashboardDto(), customers);

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Equal(4, result.TotalCustomerCount);
        Assert.Equal(2, result.RepeatCustomerCount);
        Assert.Equal(50m, result.RepeatCustomerRatePercent);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_NoCustomers_RepeatCustomerRateIsNull()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto { ProducerId = producer.Id, ProducerName = "x" };
        StubHappyPathDependencies(producer, craftProfile, new RevenueDashboardDto(), new RevenueDashboardDto(), new RevenueDashboardDto(), new List<ProducerCustomerDto>());

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Null(result.RepeatCustomerRatePercent);
    }

    [Fact]
    public async Task GetBusinessProfileAsync_CountsOnlyApprovedActiveProductsAsActive()
    {
        var producer = TestUsers.Create().WithRoles(RoleNames.Producer);
        var craftProfile = new SupplierProfileDto { ProducerId = producer.Id, ProducerName = "x" };
        StubHappyPathDependencies(producer, craftProfile, new RevenueDashboardDto(), new RevenueDashboardDto(), new RevenueDashboardDto());
        _productRepository.GetByProducerAsync(producer.Id, Arg.Any<CancellationToken>()).Returns(new List<Product>
        {
            new() { Id = Guid.NewGuid(), IsActive = true, ApprovalStatus = ProductApprovalStatus.Approved },
            new() { Id = Guid.NewGuid(), IsActive = true, ApprovalStatus = ProductApprovalStatus.Pending },
            new() { Id = Guid.NewGuid(), IsActive = false, ApprovalStatus = ProductApprovalStatus.Approved },
        });

        var result = await _service.GetBusinessProfileAsync(producer.Id, Ct);

        Assert.Equal(3, result.TotalProductCount);
        Assert.Equal(1, result.ActiveProductCount);
    }
}
