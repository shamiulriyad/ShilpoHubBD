// Regression checks for the Part 5 settlement calculation: gross revenue from multiple delivered
// order items, refund deductions, the platform-fee/producer-share/BP-share split, duplicate/overlap
// prevention across two settlement periods, and the expired-partnership guard. No database —
// in-memory fakes only. Run with `dotnet run`.
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Logistics;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

async Task CheckThrowsAsync(string name, Func<Task> action)
{
    try { await action(); Check(name, false); }
    catch (Exception) { Check(name, true); }
}

User MakeUser(string name, string role)
{
    var id = Guid.NewGuid();
    var user = new User { Id = id, FullName = name, Email = $"{name.Replace(" ", ".").ToLowerInvariant()}@test.local", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    user.UserRoles.Add(new UserRole { UserId = id, User = user, RoleId = Guid.NewGuid(), Role = new Role { Id = Guid.NewGuid(), Name = role } });
    return user;
}

OrderItem MakeOrderItem(Guid producerId, decimal lineTotal, OrderItemProducerStatus status, DateTime orderCreatedAt)
{
    var order = new Order { Id = Guid.NewGuid(), OrderNumber = $"ORD-{Guid.NewGuid():N}", CreatedAt = orderCreatedAt, UpdatedAt = orderCreatedAt };
    var product = new Product { Id = Guid.NewGuid(), ProducerId = producerId, Name = "Test Product", Slug = "test-product" };
    return new OrderItem
    {
        Id = Guid.NewGuid(), OrderId = order.Id, Order = order, ProductId = product.Id, Product = product,
        ProductName = product.Name, UnitPrice = lineTotal, Quantity = 1, LineTotal = lineTotal, ProducerStatus = status,
    };
}

var admin = MakeUser("Admin One", "SuperAdmin");
var producer = MakeUser("Producer A", "Producer");
var bp = MakeUser("BP One", "BusinessPartner");
var users = new Dictionary<Guid, User> { [admin.Id] = admin, [producer.Id] = producer, [bp.Id] = bp };

var agreementRepo = new FakeAgreementRepository(users);
var agreementService = new ProducerPartnershipAgreementService(agreementRepo, new FakeUserRepository(users));
var orderRepo = new FakeProducerOrderRepository();
var settlementRepo = new FakeSettlementRepository(users, agreementRepo);
var settlementService = new ProducerPartnershipSettlementService(settlementRepo, agreementRepo, agreementService, orderRepo);

// ===================== Set up one Active agreement, matching the worked example =====================
await agreementService.CreateFromAuctionWinAsync(Guid.NewGuid(), Guid.NewGuid(), producer.Id, bp.Id, 500000m, 12, null, CancellationToken.None);
var agreement = agreementRepo.All().Single();
await agreementService.UpdateTermsAsync(agreement.Id, new UpdateProducerPartnershipAgreementTermsRequest
{
    StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    ProducerSharePercentage = 70m, BusinessPartnerSharePercentage = 30m, PlatformFeePercentage = 10m,
    SettlementFrequency = "Monthly", PartnershipDurationMonths = 60, MinimumSettlementAmount = 500m,
}, CancellationToken.None);
await agreementService.SubmitForConfirmationAsync(agreement.Id, CancellationToken.None);
await agreementService.ConfirmAsync(agreement.Id, producer.Id, CancellationToken.None);
await agreementService.ConfirmAsync(agreement.Id, bp.Id, CancellationToken.None);
Check("agreement is Active before generating any settlement", (await agreementService.GetByIdAsync(agreement.Id, admin.Id, true, CancellationToken.None)).Status == ProducerPartnershipAgreementStatus.Active);

// ===================== Period 1: multiple orders, matching the worked example =====================
var periodStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
var periodEnd = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

orderRepo.AddItems(
    MakeOrderItem(producer.Id, 40000m, OrderItemProducerStatus.Delivered, periodStart.AddDays(2)),
    MakeOrderItem(producer.Id, 35000m, OrderItemProducerStatus.Delivered, periodStart.AddDays(5)),
    MakeOrderItem(producer.Id, 25000m, OrderItemProducerStatus.Delivered, periodStart.AddDays(10)),
    // Not yet delivered / rejected / cancelled — must NOT be counted as revenue.
    MakeOrderItem(producer.Id, 15000m, OrderItemProducerStatus.Processing, periodStart.AddDays(6)),
    MakeOrderItem(producer.Id, 9000m, OrderItemProducerStatus.Cancelled, periodStart.AddDays(7)),
    MakeOrderItem(producer.Id, 12000m, OrderItemProducerStatus.Rejected, periodStart.AddDays(8)),
    // Outside the period — must not be counted either.
    MakeOrderItem(producer.Id, 999999m, OrderItemProducerStatus.Delivered, periodStart.AddMonths(-1))
);

var settlement1 = await settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest { PeriodStart = periodStart, PeriodEnd = periodEnd }, CancellationToken.None);
Check("gross revenue counts only Delivered items within the period (100,000)", settlement1.GrossRevenue == 100000m);
Check("order count reflects the 3 delivered orders, not the excluded ones", settlement1.OrderCount == 3);
Check("no refunds this period -> refund deductions are zero", settlement1.RefundDeductions == 0m);
Check("platform fee is 10% of eligible revenue (10,000)", settlement1.PlatformFeeAmount == 10000m);
Check("net partnership revenue is gross minus the platform fee (90,000)", settlement1.NetPartnershipRevenue == 90000m);
Check("producer share is 70% of net (63,000) — matches the worked example", settlement1.ProducerShareAmount == 63000m);
Check("Business Partner share is 30% of net (27,000) — matches the worked example", settlement1.BusinessPartnerShareAmount == 27000m);
Check("settlement starts as Draft", settlement1.Status == ProducerPartnershipSettlementStatus.Draft);
Check("not below the minimum settlement amount", !settlement1.BelowMinimumThreshold);

// ===================== Duplicate / overlap prevention =====================
await CheckThrowsAsync("generating a settlement for an overlapping period is rejected (prevents double counting)",
    () => settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest { PeriodStart = periodStart.AddDays(15), PeriodEnd = periodEnd.AddDays(15) }, CancellationToken.None));

await CheckThrowsAsync("generating a settlement for a future period is rejected",
    () => settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest { PeriodStart = DateTime.UtcNow.AddDays(1), PeriodEnd = DateTime.UtcNow.AddDays(30) }, CancellationToken.None));

// ===================== Test refunds =====================
var period2Start = periodEnd.AddTicks(1);
var period2End = period2Start.AddDays(29);
orderRepo.AddItems(MakeOrderItem(producer.Id, 50000m, OrderItemProducerStatus.Delivered, period2Start.AddDays(3)));
settlementRepo.AddRefund(producer.Id, period2Start.AddDays(10), 8000m);

var settlement2 = await settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest { PeriodStart = period2Start, PeriodEnd = period2End }, CancellationToken.None);
Check("period 2 gross revenue is independent of period 1 (50,000)", settlement2.GrossRevenue == 50000m);
Check("refund deduction is applied (8,000)", settlement2.RefundDeductions == 8000m);
Check("eligible revenue after refund flows through to net partnership revenue", settlement2.NetPartnershipRevenue == (50000m - 8000m) * 0.9m);

Check("two non-overlapping settlement periods can both exist for the same agreement", settlementRepo.All().Count(s => s.AgreementId == agreement.Id) == 2);

// ===================== Approval workflow =====================
var submitted = await settlementService.SubmitForApprovalAsync(settlement1.Id, CancellationToken.None);
Check("Draft -> PendingApproval", submitted.Status == ProducerPartnershipSettlementStatus.PendingApproval);
var approved = await settlementService.ApproveAsync(settlement1.Id, admin.Id, CancellationToken.None);
Check("PendingApproval -> Approved", approved.Status == ProducerPartnershipSettlementStatus.Approved);
Check("approval never claims a payout happened — no payout reference is set", approved.PayoutReference == null);

var rejected = await settlementService.RejectAsync(settlement2.Id, new RejectProducerPartnershipSettlementRequest { Reason = "Recheck refund amount" }, CancellationToken.None);
Check("a settlement can be rejected", rejected.Status == ProducerPartnershipSettlementStatus.Rejected);

// A rejected settlement's period can be recalculated — the overlap guard excludes Rejected ones.
var settlement2Recalculated = await settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest { PeriodStart = period2Start, PeriodEnd = period2End }, CancellationToken.None);
Check("a rejected settlement's period can be recalculated", settlement2Recalculated.Id != settlement2.Id && settlement2Recalculated.GrossRevenue == 50000m);

// ===================== Test partnership expiration =====================
agreementRepo.ForceEndDateIntoThePast(agreement.Id);
await CheckThrowsAsync("an expired partnership cannot generate a new settlement for a new period",
    () => settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest
    {
        PeriodStart = period2End.AddTicks(1), PeriodEnd = period2End.AddDays(29),
    }, CancellationToken.None));

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users;
    public FakeUserRepository(Dictionary<Guid, User> users) => _users = users;
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_users.GetValueOrDefault(id));
    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct) => Task.FromResult(_users.GetValueOrDefault(id));
    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct) => Task.FromResult(_users.Values.FirstOrDefault(u => u.Email == email));
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) => Task.FromResult(_users.Values.Any(u => u.Email == email));
    public Task<bool> AnyInRoleAsync(string roleName, CancellationToken ct) => Task.FromResult(_users.Values.Any(u => u.UserRoles.Any(ur => ur.Role.Name == roleName)));
    public Task AddAsync(User user, CancellationToken ct) { _users[user.Id] = user; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeAgreementRepository : IProducerPartnershipAgreementRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly Dictionary<Guid, ProducerPartnershipAgreement> _agreements = new();
    public FakeAgreementRepository(Dictionary<Guid, User> users) => _users = users;

    private ProducerPartnershipAgreement Hydrate(ProducerPartnershipAgreement a)
    {
        a.Producer = _users[a.ProducerId];
        a.BusinessPartner = _users[a.BusinessPartnerId];
        return a;
    }

    public Task<ProducerPartnershipAgreement?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_agreements.TryGetValue(id, out var a) ? Hydrate(a) : null);
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_agreements.Values.Where(a => a.BusinessPartnerId == businessPartnerId).Select(Hydrate).ToList(), 0));
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_agreements.Values.Where(a => a.ProducerId == producerId).Select(Hydrate).ToList(), 0));
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedAllAsync(ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_agreements.Values.Select(Hydrate).ToList(), _agreements.Count));
    public Task AddAsync(ProducerPartnershipAgreement agreement, CancellationToken ct) { _agreements[agreement.Id] = agreement; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    public Task<bool> HasActiveAgreementAsync(Guid producerId, Guid? businessPartnerId, CancellationToken ct)
        => Task.FromResult(_agreements.Values.Any(a => a.ProducerId == producerId && a.Status == ProducerPartnershipAgreementStatus.Active
            && (!businessPartnerId.HasValue || a.BusinessPartnerId == businessPartnerId.Value)));
    public Task<bool> ExistsForAuctionLotAsync(Guid auctionLotId, CancellationToken ct) => Task.FromResult(_agreements.Values.Any(a => a.AuctionLotId == auctionLotId));

    public List<ProducerPartnershipAgreement> All() => _agreements.Values.ToList();
    public void ForceEndDateIntoThePast(Guid id) => _agreements[id].EndDate = DateTime.UtcNow.AddDays(-1);
}

class FakeProducerOrderRepository : IProducerOrderRepository
{
    private readonly List<OrderItem> _items = new();
    public void AddItems(params OrderItem[] items) => _items.AddRange(items);

    public Task<(List<OrderItem> Items, int TotalCount)> GetPagedByProducerAsync(Guid producerId, OrderItemProducerStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct)
        => Task.FromResult((new List<OrderItem>(), 0));
    public Task<OrderItem?> GetByIdAsync(Guid orderItemId, CancellationToken ct) => Task.FromResult<OrderItem?>(null);

    public Task<List<OrderItem>> GetByProducerAsync(Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var query = _items.Where(i => i.Product.ProducerId == producerId);
        if (fromDate.HasValue) query = query.Where(i => i.Order.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(i => i.Order.CreatedAt <= toDate.Value);
        return Task.FromResult(query.ToList());
    }

    public Task<Dictionary<Guid, (string FullName, string Email)>> GetCustomerInfoAsync(IEnumerable<Guid> userIds, CancellationToken ct)
        => Task.FromResult(new Dictionary<Guid, (string, string)>());
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeSettlementRepository : IProducerPartnershipSettlementRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly FakeAgreementRepository _agreementRepo;
    private readonly Dictionary<Guid, ProducerPartnershipSettlement> _settlements = new();
    private readonly List<(Guid ProducerId, DateTime RefundedAt, decimal Amount)> _refunds = new();
    public FakeSettlementRepository(Dictionary<Guid, User> users, FakeAgreementRepository agreementRepo) { _users = users; _agreementRepo = agreementRepo; }

    private ProducerPartnershipSettlement Hydrate(ProducerPartnershipSettlement s)
    {
        s.Agreement = _agreementRepo.All().First(a => a.Id == s.AgreementId);
        s.Agreement.Producer = _users[s.Agreement.ProducerId];
        s.Agreement.BusinessPartner = _users[s.Agreement.BusinessPartnerId];
        if (s.ApprovedByUserId.HasValue) s.ApprovedBy = _users.GetValueOrDefault(s.ApprovedByUserId.Value);
        return s;
    }

    public Task<ProducerPartnershipSettlement?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_settlements.TryGetValue(id, out var s) ? Hydrate(s) : null);
    public Task<List<ProducerPartnershipSettlement>> GetForAgreementAsync(Guid agreementId, CancellationToken ct)
        => Task.FromResult(_settlements.Values.Where(s => s.AgreementId == agreementId).Select(Hydrate).ToList());
    public Task<(List<ProducerPartnershipSettlement> Items, int TotalCount)> GetPagedAsync(ProducerPartnershipSettlementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_settlements.Values.Select(Hydrate).ToList(), _settlements.Count));

    public Task<bool> ExistsNonRejectedOverlappingAsync(Guid agreementId, DateTime periodStart, DateTime periodEnd, CancellationToken ct)
        => Task.FromResult(_settlements.Values.Any(s => s.AgreementId == agreementId
            && s.Status != ProducerPartnershipSettlementStatus.Rejected
            && s.PeriodStart < periodEnd && s.PeriodEnd > periodStart));

    public Task AddAsync(ProducerPartnershipSettlement settlement, CancellationToken ct) { _settlements[settlement.Id] = settlement; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<decimal> GetRefundDeductionsAsync(Guid producerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct)
        => Task.FromResult(_refunds.Where(r => r.ProducerId == producerId && r.RefundedAt >= periodStart && r.RefundedAt <= periodEnd).Sum(r => r.Amount));

    public void AddRefund(Guid producerId, DateTime refundedAt, decimal amount) => _refunds.Add((producerId, refundedAt, amount));
    public List<ProducerPartnershipSettlement> All() => _settlements.Values.ToList();
}
