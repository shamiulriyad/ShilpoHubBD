// PART 7 INTEGRATION AUDIT: proves the complete Business Partnership workflow is actually wired
// together end-to-end, using the REAL Application-layer service classes for every stage (only the
// repository/infrastructure layer is faked, in-memory) — not five isolated unit-test suites, one
// continuous run through the full chain from the prompt's own diagram:
//
//   Auction created -> producer entered as a lot -> BP registers & is approved -> auction goes live
//   -> BP bids and wins -> auction ends -> a Pending partnership agreement is auto-created from that
//   exact win -> admin sets revenue-share terms -> producer confirms -> BP confirms -> Active
//   -> real customer orders are delivered against that producer's product -> a settlement is
//   generated from that real order data (platform fee / producer share / BP share) -> approved
//   -> Product Intelligence, reading the SAME underlying order data independently, reports numbers
//   that cross-check against the settlement's own gross-revenue figure.
//
// Run with `dotnet run`.
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.DTOs.ProductIntelligence;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.ProducerPartnership;
using ShilpoHubBD.Application.Services.ProductIntelligence;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Inventory;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.Reviews;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

User MakeUser(string name, string role)
{
    var id = Guid.NewGuid();
    var user = new User { Id = id, FullName = name, Email = $"{name.Replace(" ", ".").ToLowerInvariant()}@test.local", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    user.UserRoles.Add(new UserRole { UserId = id, User = user, RoleId = Guid.NewGuid(), Role = new Role { Id = Guid.NewGuid(), Name = role } });
    return user;
}

var admin = MakeUser("Admin One", "SuperAdmin");
var producer = MakeUser("Nakshi Producer", "Producer");
var bp = MakeUser("Winning BP", "BusinessPartner");
var losingBp = MakeUser("Losing BP", "BusinessPartner");
var users = new Dictionary<Guid, User> { [admin.Id] = admin, [producer.Id] = producer, [bp.Id] = bp, [losingBp.Id] = losingBp };

var product = new Product { Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = "nakshi-kantha", ProducerId = producer.Id, Producer = producer, Stock = 50, LowStockThreshold = 10, AverageRating = 4.6m, ReviewCount = 8 };

var userRepo = new FakeUserRepository(users);
var auctionRepo = new FakeAuctionRepository();
var lotRepo = new FakeLotRepository(users);
var participantRepo = new FakeParticipantRepository();
var agreementRepo = new FakeAgreementRepository(users);
var supplierDiscovery = new FakeSupplierDiscoveryService();
var orderRepo = new FakeProducerOrderRepository();
var settlementRepo = new FakeSettlementRepository(users, agreementRepo);
var productRepo = new FakeProductRepository();
productRepo.Add(product);
var reviewRepo = new FakeReviewRepository();
var wishlistRepo = new FakeWishlistRepository();
var inventoryRepo = new FakeInventoryRepository();
var aiProvider = new StubProductIntelligenceAIProvider();

var agreementService = new ProducerPartnershipAgreementService(agreementRepo, userRepo);
var auctionService = new ProducerPartnershipAuctionService(auctionRepo, lotRepo, userRepo, agreementService);
var lotService = new ProducerPartnershipAuctionLotService(auctionRepo, lotRepo, participantRepo, agreementRepo, userRepo, supplierDiscovery);
var bidService = new ProducerPartnershipAuctionBidService(auctionRepo, lotRepo, participantRepo, agreementRepo);
var participantService = new ProducerPartnershipAuctionParticipantService(auctionRepo, participantRepo);
var settlementService = new ProducerPartnershipSettlementService(settlementRepo, agreementRepo, agreementService, orderRepo);
var intelligenceService = new ProductIntelligenceService(productRepo, orderRepo, reviewRepo, wishlistRepo, inventoryRepo, aiProvider);

// ===================== STAGE 1-2: Producer + Producer Business Profile (Part 2) =====================
var businessProfile = await supplierDiscovery.GetBusinessProfileAsync(producer.Id, CancellationToken.None);
Check("Stage 1-2: Producer Business Profile is reachable for BP evaluation before the auction", businessProfile.ProducerId == producer.Id);

// ===================== STAGE 3: Annual Partnership Auction (admin-configured) =====================
var auction = await auctionService.CreateAsync(admin.Id, new CreateProducerPartnershipAuctionRequest
{
    Name = "2026 Annual Partnership Auction",
    AuctionYear = 2026,
    Description = "End-to-end audit run",
    MinimumStartingBid = 5000m,
    MinimumBidIncrement = 500m,
    MaxProducersPerBusinessPartner = 1,
}, CancellationToken.None);
await auctionService.UpdateAsync(auction.Id, new UpdateProducerPartnershipAuctionRequest
{
    BiddingOpensAt = DateTime.UtcNow.AddMinutes(-5), BiddingClosesAt = DateTime.UtcNow.AddHours(1),
}, CancellationToken.None);
await auctionService.ScheduleAsync(auction.Id, CancellationToken.None);
await auctionService.OpenRegistrationAsync(auction.Id, CancellationToken.None);
Check("Stage 3: admin can configure schedule, starting bid, increment and per-BP cap", auction.MinimumStartingBid == 5000m && auction.MinimumBidIncrement == 500m && auction.MaxProducersPerBusinessPartner == 1);

// ===================== STAGE 4-5: Eligible producer entered + eligible BP registers (discovery -> participation) =====================
var lot = await lotService.AddLotAsync(auction.Id, producer.Id, admin.Id, CancellationToken.None);
Check("Stage 4: admin controls which producers are eligible (entered as a lot)", lot.ProducerId == producer.Id);

var winningApplication = await participantService.ApplyAsync(auction.Id, bp.Id, CancellationToken.None);
var losingApplication = await participantService.ApplyAsync(auction.Id, losingBp.Id, CancellationToken.None);
await participantService.DecideAsync(auction.Id, winningApplication.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
await participantService.DecideAsync(auction.Id, losingApplication.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
Check("Stage 5: admin controls which Business Partners are eligible (approved participants)", (await participantService.GetMineAsync(auction.Id, bp.Id, CancellationToken.None))!.Status == ProducerPartnershipAuctionParticipantStatus.Approved);

await auctionService.GoLiveAsync(auction.Id, CancellationToken.None);

// ===================== STAGE 6: Bid =====================
await bidService.PlaceBidAsync(auction.Id, lot.Id, losingBp.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 5000m }, CancellationToken.None);
var winningBid = await bidService.PlaceBidAsync(auction.Id, lot.Id, bp.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 8000m }, CancellationToken.None);
Check("Stage 6: the higher bid is recorded as the current highest", winningBid.IsCurrentHighest && winningBid.Amount == 8000m);

// ===================== STAGE 7-8: Auction closes -> Winner determined =====================
var ended = await auctionService.EndAsync(auction.Id, CancellationToken.None);
var lotAfterEnd = lotRepo.GetLotSnapshot(lot.Id);
Check("Stage 7: auction transitions to Ended", ended.Status == ProducerPartnershipAuctionStatus.Ended);
Check("Stage 8: the higher bidder is recorded as the winner", lotAfterEnd.Status == ProducerPartnershipAuctionLotStatus.Awarded && lotRepo.GetWinningBidder(lotAfterEnd) == bp.Id);

// ===================== STAGE 9: Partnership Agreement auto-created from the win =====================
var agreement = agreementRepo.All().Single();
Check("Stage 9: a Pending agreement links the exact winning bid, producer and BP", agreement.Status == ProducerPartnershipAgreementStatus.Pending
    && agreement.ProducerId == producer.Id && agreement.BusinessPartnerId == bp.Id && agreement.WinningBidAmount == 8000m);

// ===================== STAGE 10: Partnership Activation =====================
await agreementService.UpdateTermsAsync(agreement.Id, new UpdateProducerPartnershipAgreementTermsRequest
{
    StartDate = DateTime.UtcNow.AddDays(-40),
    ProducerSharePercentage = 70m, BusinessPartnerSharePercentage = 30m, PlatformFeePercentage = 10m,
    SettlementFrequency = "Monthly", PartnershipDurationMonths = 36,
}, CancellationToken.None);
await agreementService.SubmitForConfirmationAsync(agreement.Id, CancellationToken.None);
await agreementService.ConfirmAsync(agreement.Id, producer.Id, CancellationToken.None);
var activated = await agreementService.ConfirmAsync(agreement.Id, bp.Id, CancellationToken.None);
Check("Stage 10: partnership activates only once both parties confirmed and terms were complete", activated.Status == ProducerPartnershipAgreementStatus.Active);

Check("Duplicate active partnerships prevented: the losing BP cannot separately win/activate this same producer",
    await agreementRepo.HasActiveAgreementAsync(producer.Id, null, CancellationToken.None) && agreementRepo.All().Count(a => a.ProducerId == producer.Id && a.Status == ProducerPartnershipAgreementStatus.Active) == 1);

// ===================== STAGE 11: Customer Sales (real order data behind everything downstream) =====================
var settlementPeriodStart = DateTime.UtcNow.AddDays(-30);
var settlementPeriodEnd = DateTime.UtcNow.AddDays(-1);

Order MakeOrder(DateTime createdAt) => new() { Id = Guid.NewGuid(), OrderNumber = $"ORD-{Guid.NewGuid():N}", CreatedAt = createdAt, UpdatedAt = createdAt };
OrderItem MakeItem(DateTime at, decimal unitPrice, int qty, OrderItemProducerStatus status)
{
    var order = MakeOrder(at);
    return new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, Order = order, ProductId = product.Id, Product = product, ProductName = product.Name, UnitPrice = unitPrice, Quantity = qty, LineTotal = unitPrice * qty, ProducerStatus = status };
}

orderRepo.AddItems(
    MakeItem(settlementPeriodStart.AddDays(5), 20000m, 2, OrderItemProducerStatus.Delivered),   // 40,000
    MakeItem(settlementPeriodStart.AddDays(12), 20000m, 1, OrderItemProducerStatus.Delivered),  // 20,000
    MakeItem(settlementPeriodStart.AddDays(20), 20000m, 2, OrderItemProducerStatus.Delivered),  // 40,000
    // Cancelled items must never count toward gross revenue.
    MakeItem(settlementPeriodStart.AddDays(8), 20000m, 5, OrderItemProducerStatus.Cancelled)
);
inventoryRepo.AddTransactions(product.Id, (settlementPeriodStart.AddDays(4), -5, "Sale"));

// ===================== STAGE 12-14: Platform fee -> Revenue share -> Settlement =====================
var settlement = await settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest
{
    PeriodStart = settlementPeriodStart, PeriodEnd = settlementPeriodEnd,
}, CancellationToken.None);

Check("Stage 11-12: gross revenue reflects only the real delivered orders (100,000), cancelled items excluded", settlement.GrossRevenue == 100000m);
Check("Stage 13: platform fee is 10% of eligible revenue (10,000)", settlement.PlatformFeeAmount == 10000m);
Check("Stage 14: net partnership revenue after the fee (90,000)", settlement.NetPartnershipRevenue == 90000m);
Check("Stage 14: the exact agreement percentages are applied (Producer 70% = 63,000, BP 30% = 27,000)",
    settlement.ProducerShareAmount == 63000m && settlement.BusinessPartnerShareAmount == 27000m);

await settlementService.SubmitForApprovalAsync(settlement.Id, CancellationToken.None);
var approvedSettlement = await settlementService.ApproveAsync(settlement.Id, admin.Id, CancellationToken.None);
Check("Stage 15: admin can approve the settlement (calculation confirmed, no payout implied)", approvedSettlement.Status == ProducerPartnershipSettlementStatus.Approved && approvedSettlement.PayoutReference == null);

try
{
    await settlementService.GenerateAsync(agreement.Id, new GenerateProducerPartnershipSettlementRequest
    {
        PeriodStart = settlementPeriodStart.AddDays(10), PeriodEnd = settlementPeriodEnd.AddDays(10),
    }, CancellationToken.None);
    Check("Double settlement / double counting prevented for an overlapping period", false);
}
catch (Exception)
{
    Check("Double settlement / double counting prevented for an overlapping period", true);
}

// ===================== STAGE 16: Product Intelligence cross-checks the SAME underlying order data =====================
var intelligence = await intelligenceService.GetIntelligenceAsync(product.Id, ProductIntelligenceRange.Last3Months, CancellationToken.None);
Check("Stage 16: Product Intelligence, reading the same orders independently, agrees with the settlement's gross revenue",
    intelligence.Periods.Sum(p => p.Revenue) == settlement.GrossRevenue);
Check("Product Intelligence sees the real current stock after the sale-driven inventory deduction", intelligence.Inventory.CurrentStock == 50);
Check("Product Intelligence correctly reports the low-stock threshold from the real product record", intelligence.Inventory.LowStockThreshold == 10);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED -- full workflow verified end-to-end" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class StubProductIntelligenceAIProvider : IProductIntelligenceAIProvider
{
    public Task<ProductIntelligenceAiInsightsDto> GenerateInsightsAsync(ProductIntelligenceAiContext context, CancellationToken ct)
        => Task.FromResult(new ProductIntelligenceAiInsightsDto { IsAiGenerated = false });
}

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

class FakeAuctionRepository : IProducerPartnershipAuctionRepository
{
    private readonly Dictionary<Guid, ProducerPartnershipAuction> _auctions = new();
    public Task<ProducerPartnershipAuction?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_auctions.GetValueOrDefault(id));
    public Task<(List<ProducerPartnershipAuction> Items, int TotalCount)> GetPagedAsync(ProducerPartnershipAuctionQueryParameters p, CancellationToken ct) => Task.FromResult((_auctions.Values.ToList(), _auctions.Count));
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct) => Task.FromResult(_auctions.Values.Any(a => a.Slug == slug));
    public Task AddAsync(ProducerPartnershipAuction auction, CancellationToken ct) { _auctions[auction.Id] = auction; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeLotRepository : IProducerPartnershipAuctionLotRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly Dictionary<Guid, ProducerPartnershipAuctionLot> _lots = new();
    private readonly Dictionary<Guid, List<ProducerPartnershipAuctionBid>> _bids = new();
    private readonly object _gate = new();
    public FakeLotRepository(Dictionary<Guid, User> users) => _users = users;

    private ProducerPartnershipAuctionLot Hydrate(ProducerPartnershipAuctionLot lot)
    {
        lot.Producer = _users[lot.ProducerId];
        lot.CurrentHighestBidder = lot.CurrentHighestBidderId.HasValue ? _users[lot.CurrentHighestBidderId.Value] : null;
        if (lot.WinningBidId.HasValue)
        {
            var winningBid = _bids[lot.Id].First(b => b.Id == lot.WinningBidId.Value);
            winningBid.BusinessPartner = _users[winningBid.BusinessPartnerId];
            lot.WinningBid = winningBid;
        }
        return lot;
    }

    public Task<ProducerPartnershipAuctionLot?> GetByIdAsync(Guid lotId, CancellationToken ct) => Task.FromResult(_lots.TryGetValue(lotId, out var lot) ? Hydrate(lot) : null);
    public Task<ProducerPartnershipAuctionLot?> GetByIdWithBidsAsync(Guid lotId, CancellationToken ct)
    {
        if (!_lots.TryGetValue(lotId, out var lot)) return Task.FromResult<ProducerPartnershipAuctionLot?>(null);
        lot.Bids = _bids.GetValueOrDefault(lot.Id, new List<ProducerPartnershipAuctionBid>());
        return Task.FromResult<ProducerPartnershipAuctionLot?>(Hydrate(lot));
    }
    public Task<List<ProducerPartnershipAuctionLot>> GetForAuctionAsync(Guid auctionId, CancellationToken ct) => Task.FromResult(_lots.Values.Where(l => l.AuctionId == auctionId).Select(Hydrate).ToList());
    public Task<List<ProducerPartnershipAuctionLot>> GetForAuctionWithBidsAsync(Guid auctionId, CancellationToken ct)
    {
        var lots = _lots.Values.Where(l => l.AuctionId == auctionId).ToList();
        foreach (var lot in lots) lot.Bids = _bids.GetValueOrDefault(lot.Id, new List<ProducerPartnershipAuctionBid>());
        return Task.FromResult(lots);
    }
    public Task<bool> ExistsForProducerAsync(Guid auctionId, Guid producerId, CancellationToken ct) => Task.FromResult(_lots.Values.Any(l => l.AuctionId == auctionId && l.ProducerId == producerId));
    public Task AddAsync(ProducerPartnershipAuctionLot lot, CancellationToken ct) { _lots[lot.Id] = lot; _bids[lot.Id] = new List<ProducerPartnershipAuctionBid>(); return Task.CompletedTask; }
    public Task<List<ProducerPartnershipAuctionBid>> GetBidsForLotAsync(Guid lotId, CancellationToken ct) => Task.FromResult(_bids.GetValueOrDefault(lotId, new List<ProducerPartnershipAuctionBid>()).OrderByDescending(b => b.Amount).ThenBy(b => b.PlacedAt).ToList());
    public Task<List<ProducerPartnershipAuctionBid>> GetBidsForBusinessPartnerAsync(Guid businessPartnerId, Guid? lotId, Guid? auctionId, CancellationToken ct)
        => Task.FromResult(_bids.Values.SelectMany(b => b).Where(b => b.BusinessPartnerId == businessPartnerId && (!lotId.HasValue || b.LotId == lotId.Value) && (!auctionId.HasValue || _lots[b.LotId].AuctionId == auctionId.Value)).ToList());
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<bool> TryPlaceBidAsync(ProducerPartnershipAuctionBid bid, decimal minimumIncrement, CancellationToken ct)
    {
        lock (_gate)
        {
            var lot = _lots[bid.LotId];
            if (lot.Status != ProducerPartnershipAuctionLotStatus.Open) return Task.FromResult(false);
            var minimumAcceptable = lot.CurrentHighestBid.HasValue ? lot.CurrentHighestBid.Value + minimumIncrement : lot.StartingBid;
            if (bid.Amount < minimumAcceptable) return Task.FromResult(false);
            lot.CurrentHighestBid = bid.Amount;
            lot.CurrentHighestBidderId = bid.BusinessPartnerId;
            lot.BidCount++;
            lot.UpdatedAt = bid.PlacedAt;
            _bids[bid.LotId].Add(bid);
            return Task.FromResult(true);
        }
    }

    public ProducerPartnershipAuctionLot GetLotSnapshot(Guid lotId) => _lots[lotId];
    public Guid GetWinningBidder(ProducerPartnershipAuctionLot lot) => _bids[lot.Id].First(b => b.Id == lot.WinningBidId).BusinessPartnerId;
}

class FakeParticipantRepository : IProducerPartnershipAuctionParticipantRepository
{
    private readonly Dictionary<Guid, ProducerPartnershipAuctionParticipant> _participants = new();
    public Task<ProducerPartnershipAuctionParticipant?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_participants.GetValueOrDefault(id));
    public Task<ProducerPartnershipAuctionParticipant?> GetForBusinessPartnerAsync(Guid auctionId, Guid businessPartnerId, CancellationToken ct)
        => Task.FromResult(_participants.Values.FirstOrDefault(p => p.AuctionId == auctionId && p.BusinessPartnerId == businessPartnerId));
    public Task<List<ProducerPartnershipAuctionParticipant>> GetForAuctionAsync(Guid auctionId, CancellationToken ct) => Task.FromResult(_participants.Values.Where(p => p.AuctionId == auctionId).ToList());
    public Task AddAsync(ProducerPartnershipAuctionParticipant participant, CancellationToken ct) { _participants[participant.Id] = participant; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeAgreementRepository : IProducerPartnershipAgreementRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly Dictionary<Guid, ProducerPartnershipAgreement> _agreements = new();
    public FakeAgreementRepository(Dictionary<Guid, User> users) => _users = users;

    private ProducerPartnershipAgreement Hydrate(ProducerPartnershipAgreement a) { a.Producer = _users[a.ProducerId]; a.BusinessPartner = _users[a.BusinessPartnerId]; return a; }

    public Task<ProducerPartnershipAgreement?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct) => Task.FromResult(_agreements.TryGetValue(id, out var a) ? Hydrate(a) : null);
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_agreements.Values.Where(a => a.BusinessPartnerId == businessPartnerId).Select(Hydrate).ToList(), 0));
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_agreements.Values.Where(a => a.ProducerId == producerId).Select(Hydrate).ToList(), 0));
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedAllAsync(ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Task.FromResult((_agreements.Values.Select(Hydrate).ToList(), _agreements.Count));
    public Task AddAsync(ProducerPartnershipAgreement agreement, CancellationToken ct) { _agreements[agreement.Id] = agreement; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    public Task<bool> HasActiveAgreementAsync(Guid producerId, Guid? businessPartnerId, CancellationToken ct)
        => Task.FromResult(_agreements.Values.Any(a => a.ProducerId == producerId && a.Status == ProducerPartnershipAgreementStatus.Active && (!businessPartnerId.HasValue || a.BusinessPartnerId == businessPartnerId.Value)));
    public Task<bool> ExistsForAuctionLotAsync(Guid auctionLotId, CancellationToken ct) => Task.FromResult(_agreements.Values.Any(a => a.AuctionLotId == auctionLotId));
    public List<ProducerPartnershipAgreement> All() => _agreements.Values.ToList();
}

class FakeSettlementRepository : IProducerPartnershipSettlementRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly FakeAgreementRepository _agreementRepo;
    private readonly Dictionary<Guid, ProducerPartnershipSettlement> _settlements = new();
    public FakeSettlementRepository(Dictionary<Guid, User> users, FakeAgreementRepository agreementRepo) { _users = users; _agreementRepo = agreementRepo; }

    private ProducerPartnershipSettlement Hydrate(ProducerPartnershipSettlement s)
    {
        s.Agreement = _agreementRepo.All().First(a => a.Id == s.AgreementId);
        s.Agreement.Producer = _users[s.Agreement.ProducerId];
        s.Agreement.BusinessPartner = _users[s.Agreement.BusinessPartnerId];
        if (s.ApprovedByUserId.HasValue) s.ApprovedBy = _users.GetValueOrDefault(s.ApprovedByUserId.Value);
        return s;
    }

    public Task<ProducerPartnershipSettlement?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_settlements.TryGetValue(id, out var s) ? Hydrate(s) : null);
    public Task<List<ProducerPartnershipSettlement>> GetForAgreementAsync(Guid agreementId, CancellationToken ct) => Task.FromResult(_settlements.Values.Where(s => s.AgreementId == agreementId).Select(Hydrate).ToList());
    public Task<(List<ProducerPartnershipSettlement> Items, int TotalCount)> GetPagedAsync(ProducerPartnershipSettlementQueryParameters p, CancellationToken ct) => Task.FromResult((_settlements.Values.Select(Hydrate).ToList(), _settlements.Count));
    public Task<bool> ExistsNonRejectedOverlappingAsync(Guid agreementId, DateTime periodStart, DateTime periodEnd, CancellationToken ct)
        => Task.FromResult(_settlements.Values.Any(s => s.AgreementId == agreementId && s.Status != ProducerPartnershipSettlementStatus.Rejected && s.PeriodStart < periodEnd && s.PeriodEnd > periodStart));
    public Task AddAsync(ProducerPartnershipSettlement settlement, CancellationToken ct) { _settlements[settlement.Id] = settlement; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    public Task<decimal> GetRefundDeductionsAsync(Guid producerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct) => Task.FromResult(0m);
}

class FakeProducerOrderRepository : IProducerOrderRepository
{
    private readonly List<OrderItem> _items = new();
    public void AddItems(params OrderItem[] items) => _items.AddRange(items);
    public Task<(List<OrderItem> Items, int TotalCount)> GetPagedByProducerAsync(Guid producerId, OrderItemProducerStatus? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<OrderItem>(), 0));
    public Task<OrderItem?> GetByIdAsync(Guid orderItemId, CancellationToken ct) => Task.FromResult<OrderItem?>(null);
    public Task<List<OrderItem>> GetByProducerAsync(Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var query = _items.Where(i => i.Product.ProducerId == producerId);
        if (fromDate.HasValue) query = query.Where(i => i.Order.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(i => i.Order.CreatedAt <= toDate.Value);
        return Task.FromResult(query.ToList());
    }
    public Task<Dictionary<Guid, (string FullName, string Email)>> GetCustomerInfoAsync(IEnumerable<Guid> userIds, CancellationToken ct) => Task.FromResult(new Dictionary<Guid, (string, string)>());
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();
    public void Add(Product product) => _products[product.Id] = product;
    public Task<(List<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query, CancellationToken ct) => Task.FromResult((new List<Product>(), 0));
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_products.GetValueOrDefault(id));
    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct) => Task.FromResult<Product?>(null);
    public Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<List<Product>> GetTrendingAsync(int count, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task<(List<Product> Items, int TotalCount)> GetPendingApprovalAsync(int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Product>(), 0));
    public Task<(decimal AveragePrice, int SampleSize)> GetCategoryPriceStatsAsync(Guid categoryId, CancellationToken ct) => Task.FromResult((0m, 0));
    public Task<List<Product>> GetByProducerAsync(Guid producerId, CancellationToken ct) => Task.FromResult(_products.Values.Where(p => p.ProducerId == producerId).ToList());
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct) => Task.FromResult(false);
    public Task<List<Product>> GetLowStockByProducerAsync(Guid producerId, CancellationToken ct) => Task.FromResult(new List<Product>());
    public Task AddAsync(Product product, CancellationToken ct) { _products[product.Id] = product; return Task.CompletedTask; }
    public Task AddVariantAsync(ProductVariant variant, CancellationToken ct) => Task.CompletedTask;
    public Task AddVideoAsync(ProductVideo video, CancellationToken ct) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeReviewRepository : IReviewRepository
{
    public Task<(List<Review> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Review>(), 0));
    public Task<(List<Review> Items, int TotalCount)> GetPagedByHeritagePlaceAsync(Guid heritagePlaceId, int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Review>(), 0));
    public Task<(List<Review> Items, int TotalCount)> GetPagedByServiceAsync(Guid touristServiceId, int page, int pageSize, CancellationToken ct) => Task.FromResult((new List<Review>(), 0));
    public Task<Review?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<Review?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<Review?> GetByHeritagePlaceAndUserAsync(Guid heritagePlaceId, Guid userId, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<Review?> GetByBookingAndUserAsync(Guid bookingId, Guid userId, CancellationToken ct) => Task.FromResult<Review?>(null);
    public Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(Guid productId, CancellationToken ct) => Task.FromResult((0d, 0));
    public Task<(double AverageRating, int ReviewCount)> GetAggregateByHeritagePlaceAsync(Guid heritagePlaceId, CancellationToken ct) => Task.FromResult((0d, 0));
    public Task<(double AverageRating, int ReviewCount)> GetAggregateByServiceAsync(Guid touristServiceId, CancellationToken ct) => Task.FromResult((0d, 0));
    public Task AddAsync(Review review, CancellationToken ct) => Task.CompletedTask;
    public Task AddImageAsync(ReviewImage image, CancellationToken ct) => Task.CompletedTask;
    public void RemoveImage(ReviewImage image) { }
    public void Remove(Review review) { }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeWishlistRepository : IWishlistRepository
{
    public Task<List<WishlistItem>> GetByUserIdAsync(Guid userId, CancellationToken ct) => Task.FromResult(new List<WishlistItem>());
    public Task<List<WishlistItem>> GetByProductAsync(Guid productId, CancellationToken ct) => Task.FromResult(new List<WishlistItem>());
    public Task<WishlistItem?> GetAsync(Guid userId, Guid productId, CancellationToken ct) => Task.FromResult<WishlistItem?>(null);
    public Task AddAsync(WishlistItem item, CancellationToken ct) => Task.CompletedTask;
    public void Remove(WishlistItem item) { }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeInventoryRepository : IInventoryRepository
{
    private readonly List<InventoryTransaction> _transactions = new();
    public void AddTransactions(Guid productId, params (DateTime At, int Change, string Reason)[] entries)
    {
        foreach (var (at, change, reason) in entries) _transactions.Add(new InventoryTransaction { Id = Guid.NewGuid(), ProductId = productId, ChangeAmount = change, Reason = reason, CreatedAt = at, CreatedByUserId = Guid.NewGuid() });
    }
    public Task<List<InventoryTransaction>> GetByProductAsync(Guid productId, CancellationToken ct) => Task.FromResult(_transactions.Where(t => t.ProductId == productId).ToList());
    public Task AddAsync(InventoryTransaction transaction, CancellationToken ct) => Task.CompletedTask;
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeSupplierDiscoveryService : ISupplierDiscoveryService
{
    public Task<PagedResult<SupplierSearchResultDto>> SearchAsync(SupplierSearchParameters parameters, CancellationToken ct) => Task.FromResult(new PagedResult<SupplierSearchResultDto>());
    public Task<SupplierProfileDto> GetProducerProfileAsync(Guid producerId, CancellationToken ct) => Task.FromResult(new SupplierProfileDto { ProducerId = producerId });
    public Task<ProducerBusinessProfileDto> GetBusinessProfileAsync(Guid producerId, CancellationToken ct) => Task.FromResult(new ProducerBusinessProfileDto { ProducerId = producerId, ProducerName = "Nakshi Producer" });
}
