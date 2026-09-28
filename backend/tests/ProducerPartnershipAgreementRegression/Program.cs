// Regression checks for the Part 4 auction-winner -> partnership workflow: term-setting, the
// two-sided confirmation state machine, duplicate-active-partnership prevention, suspend/resume,
// cancellation, and the lazy-expiry settlement guard. No database — in-memory fakes only, same
// pattern as ProductSearchRegression / ProducerPartnershipAuctionRegression. Run with `dotnet run`.
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
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
var producer = MakeUser("Producer A", "Producer");
var bp1 = MakeUser("BP One", "BusinessPartner");
var bp2 = MakeUser("BP Two", "BusinessPartner");

var users = new Dictionary<Guid, User> { [admin.Id] = admin, [producer.Id] = producer, [bp1.Id] = bp1, [bp2.Id] = bp2 };
var userRepo = new FakeUserRepository(users);
var agreementRepo = new FakeAgreementRepository(users);
var service = new ProducerPartnershipAgreementService(agreementRepo, userRepo);

// ===================== Setup: two competing proposals for the same producer =====================
var auctionId = Guid.NewGuid();
await service.CreateFromAuctionWinAsync(auctionId, Guid.NewGuid(), producer.Id, bp1.Id, 2000m, 12, 70m, CancellationToken.None);
await service.CreateFromAuctionWinAsync(auctionId, Guid.NewGuid(), producer.Id, bp2.Id, 1500m, 12, 70m, CancellationToken.None);
var agreementA = agreementRepo.All().Single(a => a.BusinessPartnerId == bp1.Id);
var agreementB = agreementRepo.All().Single(a => a.BusinessPartnerId == bp2.Id);

Check("both proposals start Pending", agreementA.Status == ProducerPartnershipAgreementStatus.Pending && agreementB.Status == ProducerPartnershipAgreementStatus.Pending);
Check("the winning bid amount and the default revenue-share default are stored separately", agreementA.WinningBidAmount == 2000m && agreementA.ProducerSharePercentage == 70m);
Check("Producer <-> Business Partner relationship recorded correctly", agreementA.ProducerId == producer.Id && agreementA.BusinessPartnerId == bp1.Id);

// ===================== Term-setting and validation =====================
await CheckThrowsAsync("cannot submit for confirmation before the shares sum to 100%",
    () => service.SubmitForConfirmationAsync(agreementA.Id, CancellationToken.None));

await CheckThrowsAsync("Producer share + BP share that don't sum to 100% are rejected (platform fee is a separate, independent percentage of gross)",
    () => service.UpdateTermsAsync(agreementA.Id, new UpdateProducerPartnershipAgreementTermsRequest
    {
        ProducerSharePercentage = 70m, BusinessPartnerSharePercentage = 25m, PlatformFeePercentage = 10m, // 70 + 25 = 95, not 100
    }, CancellationToken.None));

await service.UpdateTermsAsync(agreementA.Id, new UpdateProducerPartnershipAgreementTermsRequest
{
    ProducerSharePercentage = 70m, BusinessPartnerSharePercentage = 30m, PlatformFeePercentage = 5m,
    SettlementFrequency = "Monthly", PartnershipDurationMonths = 6,
}, CancellationToken.None);

var submittedA = await service.SubmitForConfirmationAsync(agreementA.Id, CancellationToken.None);
Check("Pending -> AwaitingProducerConfirmation once terms are complete", submittedA.Status == ProducerPartnershipAgreementStatus.AwaitingProducerConfirmation);

await CheckThrowsAsync("terms can no longer be edited once submitted",
    () => service.UpdateTermsAsync(agreementA.Id, new UpdateProducerPartnershipAgreementTermsRequest { AgreementTerms = "late edit" }, CancellationToken.None));

// ===================== Confirmation must come from the right party, in the right order =====================
await CheckThrowsAsync("the Business Partner cannot confirm before the producer has",
    () => service.ConfirmAsync(agreementA.Id, bp1.Id, CancellationToken.None),
    ex => ex is UnauthorizedAccessException);

await CheckThrowsAsync("an unrelated user cannot confirm on the producer's behalf",
    () => service.ConfirmAsync(agreementA.Id, bp2.Id, CancellationToken.None),
    ex => ex is UnauthorizedAccessException);

var producerConfirmed = await service.ConfirmAsync(agreementA.Id, producer.Id, CancellationToken.None);
Check("producer confirmation moves it to AwaitingBPConfirmation", producerConfirmed.Status == ProducerPartnershipAgreementStatus.AwaitingBPConfirmation);
Check("producer confirmation timestamp recorded", producerConfirmed.ProducerConfirmedAt != null);

var active = await service.ConfirmAsync(agreementA.Id, bp1.Id, CancellationToken.None);
Check("Business Partner confirmation activates the partnership", active.Status == ProducerPartnershipAgreementStatus.Active);
Check("start date defaults to now and end date is computed from the duration", active.StartDate != null && active.EndDate != null && active.EndDate > active.StartDate);

// ===================== Duplicate active partnerships are prevented =====================
await service.UpdateTermsAsync(agreementB.Id, new UpdateProducerPartnershipAgreementTermsRequest
{
    ProducerSharePercentage = 60m, BusinessPartnerSharePercentage = 40m, PlatformFeePercentage = 10m, SettlementFrequency = "Quarterly",
}, CancellationToken.None);
await service.SubmitForConfirmationAsync(agreementB.Id, CancellationToken.None);
await service.ConfirmAsync(agreementB.Id, producer.Id, CancellationToken.None);

await CheckThrowsAsync("activating a second partnership for a producer who already has an active one is blocked",
    () => service.ConfirmAsync(agreementB.Id, bp2.Id, CancellationToken.None));

var stillWaiting = await service.GetByIdAsync(agreementB.Id, admin.Id, isAdmin: true, CancellationToken.None);
Check("the blocked proposal is left exactly as it was, not half-activated", stillWaiting.Status == ProducerPartnershipAgreementStatus.AwaitingBPConfirmation && stillWaiting.BusinessPartnerConfirmedAt == null);

var cancelledB = await service.CancelAsync(agreementB.Id, admin.Id, isAdmin: true, new TerminateProducerPartnershipAgreementRequest { Reason = "Producer already partnered." }, CancellationToken.None);
Check("the losing proposal can be cancelled by an admin", cancelledB.Status == ProducerPartnershipAgreementStatus.Cancelled);

// ===================== Settlement guard =====================
await service.EnsureCanRecordSettlementAsync(agreementA.Id, CancellationToken.None); // should not throw
Check("an active partnership is eligible to record a settlement", true);

var suspended = await service.SuspendAsync(agreementA.Id, "Under dispute review", CancellationToken.None);
Check("an active partnership can be suspended", suspended.Status == ProducerPartnershipAgreementStatus.Suspended);

await CheckThrowsAsync("a suspended partnership cannot generate a settlement record",
    () => service.EnsureCanRecordSettlementAsync(agreementA.Id, CancellationToken.None));

var resumed = await service.ResumeAsync(agreementA.Id, CancellationToken.None);
Check("a suspended partnership can resume to active", resumed.Status == ProducerPartnershipAgreementStatus.Active);

// ===================== Lazy expiry =====================
agreementRepo.ForceEndDateIntoThePast(agreementA.Id);
await CheckThrowsAsync("an expired partnership cannot generate a new settlement record",
    () => service.EnsureCanRecordSettlementAsync(agreementA.Id, CancellationToken.None));

var expired = await service.GetByIdAsync(agreementA.Id, admin.Id, isAdmin: true, CancellationToken.None);
Check("the agreement is lazily transitioned to Expired once its end date has passed", expired.Status == ProducerPartnershipAgreementStatus.Expired);

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
        => Task.FromResult(_agreements.Values.Any(a => a.ProducerId == producerId && a.Status == ProducerPartnershipAgreementStatus.Active
            && (!businessPartnerId.HasValue || a.BusinessPartnerId == businessPartnerId.Value)));

    public Task<bool> ExistsForAuctionLotAsync(Guid auctionLotId, CancellationToken ct)
        => Task.FromResult(_agreements.Values.Any(a => a.AuctionLotId == auctionLotId));

    public List<ProducerPartnershipAgreement> All() => _agreements.Values.ToList();
    public void ForceEndDateIntoThePast(Guid id) => _agreements[id].EndDate = DateTime.UtcNow.AddDays(-1);
}
