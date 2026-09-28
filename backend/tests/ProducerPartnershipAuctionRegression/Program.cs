// Regression checks for the Producer Partnership Auction module (Part 3): lifecycle, bid
// validation, unauthorized access, expired-auction rejection, concurrent bidding and the
// MaxProducersPerBusinessPartner winner-fallback algorithm. No database — in-memory fakes only,
// same pattern as ProductSearchRegression. Run with `dotnet run`.
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

async Task CheckThrowsAsync(string name, Func<Task> action, Func<Exception, bool>? matches = null)
{
    try
    {
        await action();
        Check(name, false);
    }
    catch (Exception ex)
    {
        Check(name, matches?.Invoke(ex) ?? true);
    }
}

User MakeUser(string name, string role)
{
    var id = Guid.NewGuid();
    var user = new User { Id = id, FullName = name, Email = $"{name.Replace(" ", ".").ToLowerInvariant()}@test.local", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    user.UserRoles.Add(new UserRole { UserId = id, User = user, RoleId = Guid.NewGuid(), Role = new Role { Id = Guid.NewGuid(), Name = role } });
    return user;
}

var admin = MakeUser("Admin One", "SuperAdmin");
var producerA = MakeUser("Producer A", "Producer");
var producerB = MakeUser("Producer B", "Producer");
var producerC = MakeUser("Producer C", "Producer");
var producerD = MakeUser("Producer D", "Producer");
var bp1 = MakeUser("BP One", "BusinessPartner");
var bp2 = MakeUser("BP Two", "BusinessPartner");
var bp3 = MakeUser("BP Three", "BusinessPartner");

var users = new Dictionary<Guid, User> { [admin.Id] = admin, [producerA.Id] = producerA, [producerB.Id] = producerB, [producerC.Id] = producerC, [producerD.Id] = producerD, [bp1.Id] = bp1, [bp2.Id] = bp2, [bp3.Id] = bp3 };

var userRepo = new FakeUserRepository(users);
var auctionRepo = new FakeAuctionRepository();
var lotRepo = new FakeLotRepository(users);
var participantRepo = new FakeParticipantRepository();
var agreementRepo = new FakeAgreementRepository(users);
var supplierDiscovery = new FakeSupplierDiscoveryService();

var agreementService = new ProducerPartnershipAgreementService(agreementRepo, userRepo);
var auctionService = new ProducerPartnershipAuctionService(auctionRepo, lotRepo, userRepo, agreementService);
var lotService = new ProducerPartnershipAuctionLotService(auctionRepo, lotRepo, participantRepo, agreementRepo, userRepo, supplierDiscovery);
var bidService = new ProducerPartnershipAuctionBidService(auctionRepo, lotRepo, participantRepo, agreementRepo);
var participantService = new ProducerPartnershipAuctionParticipantService(auctionRepo, participantRepo);

// ===================== 1. Auction lifecycle =====================
var auction = await auctionService.CreateAsync(admin.Id, new CreateProducerPartnershipAuctionRequest
{
    Name = "2026 Partnership Auction",
    AuctionYear = 2026,
    Description = "Annual auction",
    MinimumStartingBid = 1000m,
    MinimumBidIncrement = 100m,
    MaxProducersPerBusinessPartner = 1,
}, CancellationToken.None);
Check("auction created in Draft", auction.Status == ProducerPartnershipAuctionStatus.Draft);

await CheckThrowsAsync("cannot open registration directly from Draft", () => auctionService.OpenRegistrationAsync(auction.Id, CancellationToken.None));
await CheckThrowsAsync("cannot schedule without bidding dates set", () => auctionService.ScheduleAsync(auction.Id, CancellationToken.None));

await auctionService.UpdateAsync(auction.Id, new UpdateProducerPartnershipAuctionRequest
{
    BiddingOpensAt = DateTime.UtcNow.AddMinutes(-5),
    BiddingClosesAt = DateTime.UtcNow.AddHours(1),
}, CancellationToken.None);

var scheduled = await auctionService.ScheduleAsync(auction.Id, CancellationToken.None);
Check("Draft -> Scheduled", scheduled.Status == ProducerPartnershipAuctionStatus.Scheduled);

var regOpen = await auctionService.OpenRegistrationAsync(auction.Id, CancellationToken.None);
Check("Scheduled -> RegistrationOpen", regOpen.Status == ProducerPartnershipAuctionStatus.RegistrationOpen);

await CheckThrowsAsync("cannot go live with no lots", () => auctionService.GoLiveAsync(auction.Id, CancellationToken.None));

var lotA = await lotService.AddLotAsync(auction.Id, producerA.Id, admin.Id, CancellationToken.None);
var lotB = await lotService.AddLotAsync(auction.Id, producerB.Id, admin.Id, CancellationToken.None);
Check("lot starting bid defaults to the auction minimum", lotA.StartingBid == 1000m);

await CheckThrowsAsync("cannot enter the same producer twice", () => lotService.AddLotAsync(auction.Id, producerA.Id, admin.Id, CancellationToken.None));

var p1 = await participantService.ApplyAsync(auction.Id, bp1.Id, CancellationToken.None);
var p2 = await participantService.ApplyAsync(auction.Id, bp2.Id, CancellationToken.None);
var p3 = await participantService.ApplyAsync(auction.Id, bp3.Id, CancellationToken.None); // left unapproved on purpose
await CheckThrowsAsync("cannot apply twice", () => participantService.ApplyAsync(auction.Id, bp1.Id, CancellationToken.None));

await participantService.DecideAsync(auction.Id, p1.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
await participantService.DecideAsync(auction.Id, p2.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
var rejected = await participantService.DecideAsync(auction.Id, p3.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = false, Notes = "not eligible" }, CancellationToken.None);
Check("admin can reject a participant", rejected.Status == ProducerPartnershipAuctionParticipantStatus.Rejected);

var live = await auctionService.GoLiveAsync(auction.Id, CancellationToken.None);
Check("RegistrationOpen -> Live", live.Status == ProducerPartnershipAuctionStatus.Live);
Check("lots move to Open once the auction goes live", lotRepo.GetLotSnapshot(lotA.Id).Status == ProducerPartnershipAuctionLotStatus.Open);

// ===================== 2 & 3. Multiple BPs bidding + bid validation =====================
await CheckThrowsAsync("bid below the starting bid is rejected", () => bidService.PlaceBidAsync(auction.Id, lotA.Id, bp1.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 500m }, CancellationToken.None));

var bid1 = await bidService.PlaceBidAsync(auction.Id, lotA.Id, bp1.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1000m }, CancellationToken.None);
Check("first bid at the starting price is accepted", bid1.IsCurrentHighest);

await CheckThrowsAsync("bid that doesn't beat the current highest by the minimum increment is rejected",
    () => bidService.PlaceBidAsync(auction.Id, lotA.Id, bp2.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1050m }, CancellationToken.None));

var bid2 = await bidService.PlaceBidAsync(auction.Id, lotA.Id, bp2.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1100m }, CancellationToken.None);
Check("a higher bid from a different Business Partner is accepted", bid2.IsCurrentHighest);

var lotAAfter = lotRepo.GetLotSnapshot(lotA.Id);
Check("the lot reflects BP2 as the new current highest bidder", lotAAfter.CurrentHighestBidderId == bp2.Id && lotAAfter.CurrentHighestBid == 1100m);
Check("bid count is 2", lotAAfter.BidCount == 2);

await CheckThrowsAsync("an unapproved participant cannot bid", () => bidService.PlaceBidAsync(auction.Id, lotA.Id, bp3.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 2000m }, CancellationToken.None),
    ex => ex is UnauthorizedAccessException);

agreementRepo.SetActiveAgreement(producerB.Id, bp1.Id);
await CheckThrowsAsync("a Business Partner already partnered with this producer cannot bid on them again",
    () => bidService.PlaceBidAsync(auction.Id, lotB.Id, bp1.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1000m }, CancellationToken.None));

var bidLotB = await bidService.PlaceBidAsync(auction.Id, lotB.Id, bp2.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1000m }, CancellationToken.None);
Check("a different, unrestricted Business Partner can still bid on that producer", bidLotB.IsCurrentHighest);

// ===================== 4. Expired auction =====================
auctionRepo.ForceBiddingClosesAt(auction.Id, DateTime.UtcNow.AddMinutes(-1));
await CheckThrowsAsync("a bid is rejected once BiddingClosesAt has passed, even while Status is still Live",
    () => bidService.PlaceBidAsync(auction.Id, lotA.Id, bp1.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1500m }, CancellationToken.None));
auctionRepo.ForceBiddingClosesAt(auction.Id, DateTime.UtcNow.AddHours(1));

// ===================== 5. Unauthorized access (role-boundary rules the controller normally enforces via [Authorize]) =====================
await CheckThrowsAsync("a non-participant cannot view lot details",
    () => lotService.GetLotDetailAsync(auction.Id, lotA.Id, bp3.Id, isAdmin: false, CancellationToken.None),
    ex => ex is UnauthorizedAccessException);

// ===================== 6. Concurrent / rapid bidding =====================
{
    var raceAuction = await auctionService.CreateAsync(admin.Id, new CreateProducerPartnershipAuctionRequest
    {
        Name = "Race Auction",
        AuctionYear = 2026,
        Description = "race",
        MinimumStartingBid = 1000m,
        MinimumBidIncrement = 50m,
    }, CancellationToken.None);
    await auctionService.UpdateAsync(raceAuction.Id, new UpdateProducerPartnershipAuctionRequest { BiddingOpensAt = DateTime.UtcNow.AddMinutes(-5), BiddingClosesAt = DateTime.UtcNow.AddHours(1) }, CancellationToken.None);
    await auctionService.ScheduleAsync(raceAuction.Id, CancellationToken.None);
    await auctionService.OpenRegistrationAsync(raceAuction.Id, CancellationToken.None);
    var raceLot = await lotService.AddLotAsync(raceAuction.Id, producerC.Id, admin.Id, CancellationToken.None);
    var rp1 = await participantService.ApplyAsync(raceAuction.Id, bp1.Id, CancellationToken.None);
    var rp2 = await participantService.ApplyAsync(raceAuction.Id, bp2.Id, CancellationToken.None);
    await participantService.DecideAsync(raceAuction.Id, rp1.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
    await participantService.DecideAsync(raceAuction.Id, rp2.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
    await auctionService.GoLiveAsync(raceAuction.Id, CancellationToken.None);

    // Both bidders read the same starting state (no highest bid yet) and race to bid the same,
    // just-acceptable amount at the same instant — the real "two browser tabs" scenario. The fake
    // repository's TryPlaceBidAsync guards the read-check-write with a lock, exactly like the real
    // repository's single atomic guarded SQL UPDATE does in PostgreSQL.
    var raceResults = await Task.WhenAll(
        Task.Run(() => SafeBid(bidService, raceAuction.Id, raceLot.Id, bp1.Id, 1000m)),
        Task.Run(() => SafeBid(bidService, raceAuction.Id, raceLot.Id, bp2.Id, 1000m)));

    Check("exactly one of two identical concurrent bids wins the race", raceResults.Count(r => r.Ok) == 1);
    // The losing bid can be rejected by either guard depending on exact timing: the atomic
    // repository-level guard ("higher bid") or the service's own pre-check re-reading the now
    // already-updated lot ("must be at least") — both are correct, equally valid rejections.
    Check("the losing concurrent bid failed with a conflict, not a silent corruption",
        raceResults.Any(r => !r.Ok && (r.Error!.Contains("higher bid") || r.Error!.Contains("must be at least"))));
    Check("the lot's bid count reflects only the one bid that actually won", lotRepo.GetLotSnapshot(raceLot.Id).BidCount == 1);
}

// ===================== 7. Winner determination + MaxProducersPerBusinessPartner fallback =====================
{
    var capAuction = await auctionService.CreateAsync(admin.Id, new CreateProducerPartnershipAuctionRequest
    {
        Name = "Cap Auction",
        AuctionYear = 2026,
        Description = "cap test",
        MinimumStartingBid = 1000m,
        MinimumBidIncrement = 100m,
        MaxProducersPerBusinessPartner = 1,
    }, CancellationToken.None);
    await auctionService.UpdateAsync(capAuction.Id, new UpdateProducerPartnershipAuctionRequest { BiddingOpensAt = DateTime.UtcNow.AddMinutes(-5), BiddingClosesAt = DateTime.UtcNow.AddHours(1) }, CancellationToken.None);
    await auctionService.ScheduleAsync(capAuction.Id, CancellationToken.None);
    await auctionService.OpenRegistrationAsync(capAuction.Id, CancellationToken.None);
    var lot1 = await lotService.AddLotAsync(capAuction.Id, producerC.Id, admin.Id, CancellationToken.None);
    var lot2 = await lotService.AddLotAsync(capAuction.Id, producerD.Id, admin.Id, CancellationToken.None);
    var cp1 = await participantService.ApplyAsync(capAuction.Id, bp1.Id, CancellationToken.None);
    var cp2 = await participantService.ApplyAsync(capAuction.Id, bp2.Id, CancellationToken.None);
    await participantService.DecideAsync(capAuction.Id, cp1.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
    await participantService.DecideAsync(capAuction.Id, cp2.Id, admin.Id, new DecideProducerPartnershipAuctionParticipantRequest { Approve = true }, CancellationToken.None);
    await auctionService.GoLiveAsync(capAuction.Id, CancellationToken.None);

    // BP1 ends up the top bidder on BOTH lots, but with a higher amount on lot1. With a cap of 1,
    // ending the auction should award BP1 the higher-value lot1 and let lot2 fall through to BP2
    // (the next-highest bidder there) instead of leaving it unsold.
    await bidService.PlaceBidAsync(capAuction.Id, lot1.Id, bp2.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1000m }, CancellationToken.None);
    await bidService.PlaceBidAsync(capAuction.Id, lot1.Id, bp1.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 2000m }, CancellationToken.None);
    await bidService.PlaceBidAsync(capAuction.Id, lot2.Id, bp2.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1000m }, CancellationToken.None);
    await bidService.PlaceBidAsync(capAuction.Id, lot2.Id, bp1.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 1200m }, CancellationToken.None);

    var ended = await auctionService.EndAsync(capAuction.Id, CancellationToken.None);
    Check("auction transitions to Ended", ended.Status == ProducerPartnershipAuctionStatus.Ended);

    var lot1Final = lotRepo.GetLotSnapshot(lot1.Id);
    var lot2Final = lotRepo.GetLotSnapshot(lot2.Id);
    Check("the higher-value lot is awarded to BP1", lot1Final.Status == ProducerPartnershipAuctionLotStatus.Awarded && lotRepo.GetWinningBidder(lot1Final) == bp1.Id);
    Check("the lower-value lot falls through to BP2 once BP1 hits the cap", lot2Final.Status == ProducerPartnershipAuctionLotStatus.Awarded && lotRepo.GetWinningBidder(lot2Final) == bp2.Id);

    await CheckThrowsAsync("a bid can no longer be placed once the auction has ended",
        () => bidService.PlaceBidAsync(capAuction.Id, lot1.Id, bp2.Id, new PlaceProducerPartnershipAuctionBidRequest { Amount = 5000m }, CancellationToken.None));

    // Part 4: ending the auction should have auto-created a Pending partnership proposal per
    // awarded lot, snapshotting the winning bid amount — never assuming it equals a revenue share.
    var agreementForLot1 = agreementRepo.FindByAuctionLotId(lot1.Id);
    var agreementForLot2 = agreementRepo.FindByAuctionLotId(lot2.Id);
    Check("an agreement is auto-created for each awarded lot", agreementForLot1 != null && agreementForLot2 != null);
    Check("the agreement starts Pending, not Active", agreementForLot1!.Status == ProducerPartnershipAgreementStatus.Pending && agreementForLot2!.Status == ProducerPartnershipAgreementStatus.Pending);
    Check("the agreement snapshots the winning bid amount, separate from any revenue-share field", agreementForLot1.WinningBidAmount == 2000m && agreementForLot2!.WinningBidAmount == 1000m);
    Check("no revenue share is assumed from the bid amount", agreementForLot1.BusinessPartnerSharePercentage == null && agreementForLot1.PlatformFeePercentage == null);
}

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

static async Task<(bool Ok, string? Error)> SafeBid(IProducerPartnershipAuctionBidService service, Guid auctionId, Guid lotId, Guid businessPartnerId, decimal amount)
{
    try
    {
        await service.PlaceBidAsync(auctionId, lotId, businessPartnerId, new PlaceProducerPartnershipAuctionBidRequest { Amount = amount }, CancellationToken.None);
        return (true, null);
    }
    catch (Exception ex)
    {
        return (false, ex.Message);
    }
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
    public Task<(List<ProducerPartnershipAuction> Items, int TotalCount)> GetPagedAsync(ProducerPartnershipAuctionQueryParameters parameters, CancellationToken ct)
        => Task.FromResult((_auctions.Values.ToList(), _auctions.Count));
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct) => Task.FromResult(_auctions.Values.Any(a => a.Slug == slug));
    public Task AddAsync(ProducerPartnershipAuction auction, CancellationToken ct) { _auctions[auction.Id] = auction; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;

    public void ForceBiddingClosesAt(Guid id, DateTime at) => _auctions[id].BiddingClosesAt = at;
}

class FakeLotRepository : IProducerPartnershipAuctionLotRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly Dictionary<Guid, ProducerPartnershipAuctionLot> _lots = new();
    private readonly Dictionary<Guid, List<ProducerPartnershipAuctionBid>> _bids = new();
    private readonly object _gate = new();

    public FakeLotRepository(Dictionary<Guid, User> users) => _users = users;

    // Mirrors what the real repository's .Include()/.ThenInclude() calls give it for free on every
    // read: Producer/CurrentHighestBidder/WinningBid+its BusinessPartner, all hydrated from the
    // current state, never trusted from whatever was set at Add-time.
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

    public Task<ProducerPartnershipAuctionLot?> GetByIdAsync(Guid lotId, CancellationToken ct)
        => Task.FromResult(_lots.TryGetValue(lotId, out var lot) ? Hydrate(lot) : null);

    public Task<ProducerPartnershipAuctionLot?> GetByIdWithBidsAsync(Guid lotId, CancellationToken ct)
    {
        if (!_lots.TryGetValue(lotId, out var lot))
        {
            return Task.FromResult<ProducerPartnershipAuctionLot?>(null);
        }

        lot.Bids = _bids.GetValueOrDefault(lot.Id, new List<ProducerPartnershipAuctionBid>());
        return Task.FromResult<ProducerPartnershipAuctionLot?>(Hydrate(lot));
    }

    public Task<List<ProducerPartnershipAuctionLot>> GetForAuctionAsync(Guid auctionId, CancellationToken ct)
        => Task.FromResult(_lots.Values.Where(l => l.AuctionId == auctionId).Select(Hydrate).ToList());

    public Task<List<ProducerPartnershipAuctionLot>> GetForAuctionWithBidsAsync(Guid auctionId, CancellationToken ct)
    {
        var lots = _lots.Values.Where(l => l.AuctionId == auctionId).ToList();
        foreach (var lot in lots)
        {
            lot.Bids = _bids.GetValueOrDefault(lot.Id, new List<ProducerPartnershipAuctionBid>());
        }

        return Task.FromResult(lots);
    }

    public Task<bool> ExistsForProducerAsync(Guid auctionId, Guid producerId, CancellationToken ct)
        => Task.FromResult(_lots.Values.Any(l => l.AuctionId == auctionId && l.ProducerId == producerId));

    public Task AddAsync(ProducerPartnershipAuctionLot lot, CancellationToken ct) { _lots[lot.Id] = lot; _bids[lot.Id] = new List<ProducerPartnershipAuctionBid>(); return Task.CompletedTask; }

    public Task<List<ProducerPartnershipAuctionBid>> GetBidsForLotAsync(Guid lotId, CancellationToken ct)
        => Task.FromResult(_bids.GetValueOrDefault(lotId, new List<ProducerPartnershipAuctionBid>())
            .Select(b => { b.BusinessPartner = _users[b.BusinessPartnerId]; return b; })
            .OrderByDescending(b => b.Amount).ThenBy(b => b.PlacedAt).ToList());

    public Task<List<ProducerPartnershipAuctionBid>> GetBidsForBusinessPartnerAsync(Guid businessPartnerId, Guid? lotId, Guid? auctionId, CancellationToken ct)
        => Task.FromResult(_bids.Values.SelectMany(b => b).Where(b => b.BusinessPartnerId == businessPartnerId
            && (!lotId.HasValue || b.LotId == lotId.Value)
            && (!auctionId.HasValue || _lots[b.LotId].AuctionId == auctionId.Value))
            .Select(b => { b.Lot = Hydrate(_lots[b.LotId]); return b; }).ToList());

    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;

    // Faithfully reproduces the real repository's contract: a single atomic critical section that
    // re-checks "lot still Open" and "amount still beats the current highest by the increment" and,
    // only if that holds, applies the update — mirroring what the real repository gets for free from
    // a single guarded SQL UPDATE statement in PostgreSQL.
    public Task<bool> TryPlaceBidAsync(ProducerPartnershipAuctionBid bid, decimal minimumIncrement, CancellationToken ct)
    {
        lock (_gate)
        {
            var lot = _lots[bid.LotId];
            if (lot.Status != ProducerPartnershipAuctionLotStatus.Open)
            {
                return Task.FromResult(false);
            }

            var minimumAcceptable = lot.CurrentHighestBid.HasValue ? lot.CurrentHighestBid.Value + minimumIncrement : lot.StartingBid;
            if (bid.Amount < minimumAcceptable)
            {
                return Task.FromResult(false);
            }

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
    public Task<List<ProducerPartnershipAuctionParticipant>> GetForAuctionAsync(Guid auctionId, CancellationToken ct)
        => Task.FromResult(_participants.Values.Where(p => p.AuctionId == auctionId).ToList());
    public Task AddAsync(ProducerPartnershipAuctionParticipant participant, CancellationToken ct) { _participants[participant.Id] = participant; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

class FakeAgreementRepository : IProducerPartnershipAgreementRepository
{
    private readonly Dictionary<Guid, User> _users;
    private readonly Dictionary<Guid, ProducerPartnershipAgreement> _agreements = new();
    private readonly HashSet<(Guid Producer, Guid BusinessPartner)> _preSetActive = new();

    public FakeAgreementRepository(Dictionary<Guid, User> users) => _users = users;

    private ProducerPartnershipAgreement Hydrate(ProducerPartnershipAgreement a)
    {
        a.Producer = _users[a.ProducerId];
        a.BusinessPartner = _users[a.BusinessPartnerId];
        return a;
    }

    public Task<ProducerPartnershipAgreement?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_agreements.TryGetValue(id, out var a) ? Hydrate(a) : null);

    private Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> Page(IEnumerable<ProducerPartnershipAgreement> query)
    {
        var items = query.Select(Hydrate).ToList();
        return Task.FromResult((items, items.Count));
    }

    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Page(_agreements.Values.Where(a => a.BusinessPartnerId == businessPartnerId));
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Page(_agreements.Values.Where(a => a.ProducerId == producerId));
    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedAllAsync(ProducerPartnershipAgreementQueryParameters p, CancellationToken ct)
        => Page(_agreements.Values);

    public Task AddAsync(ProducerPartnershipAgreement agreement, CancellationToken ct) { _agreements[agreement.Id] = agreement; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<bool> HasActiveAgreementAsync(Guid producerId, Guid? businessPartnerId, CancellationToken ct)
        => Task.FromResult(
            _agreements.Values.Any(a => a.ProducerId == producerId && a.Status == ProducerPartnershipAgreementStatus.Active
                && (!businessPartnerId.HasValue || a.BusinessPartnerId == businessPartnerId.Value))
            || (businessPartnerId.HasValue
                ? _preSetActive.Contains((producerId, businessPartnerId.Value))
                : _preSetActive.Any(a => a.Producer == producerId)));

    public Task<bool> ExistsForAuctionLotAsync(Guid auctionLotId, CancellationToken ct)
        => Task.FromResult(_agreements.Values.Any(a => a.AuctionLotId == auctionLotId));

    public void SetActiveAgreement(Guid producerId, Guid businessPartnerId) => _preSetActive.Add((producerId, businessPartnerId));
    public ProducerPartnershipAgreement? FindByAuctionLotId(Guid auctionLotId) => _agreements.Values.FirstOrDefault(a => a.AuctionLotId == auctionLotId);
}

class FakeSupplierDiscoveryService : ISupplierDiscoveryService
{
    public Task<PagedResult<SupplierSearchResultDto>> SearchAsync(SupplierSearchParameters parameters, CancellationToken ct)
        => Task.FromResult(new PagedResult<SupplierSearchResultDto>());
    public Task<SupplierProfileDto> GetProducerProfileAsync(Guid producerId, CancellationToken ct)
        => Task.FromResult(new SupplierProfileDto { ProducerId = producerId });
    public Task<ProducerBusinessProfileDto> GetBusinessProfileAsync(Guid producerId, CancellationToken ct)
        => Task.FromResult(new ProducerBusinessProfileDto { ProducerId = producerId, ProducerName = "Stub Producer" });
}
