using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Learning;
using ShilpoHubBD.Application.Validators.Learning;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Learning;

var ct = CancellationToken.None;
var userId = Guid.NewGuid();
var adminId = Guid.NewGuid();
var repository = new MentorFixture();
var service = new MentorService(repository, null!);
var request = new BecomeMentorRequest { Bio = "Craft teacher", Expertise = "Handloom weaving", YearsOfExperience = 12, ProofImageUrl = "/uploads/images/0123456789abcdef0123456789abcdef.jpg" };
Check("Photo proof validator accepts an uploaded image path", new BecomeMentorRequestValidator().Validate(request).IsValid);
request.ProofImageUrl = "https://example.com/unverified.jpg";
Check("Photo proof validator rejects external/unuploaded proof", !new BecomeMentorRequestValidator().Validate(request).IsValid);
request.ProofImageUrl = "/uploads/images/0123456789abcdef0123456789abcdef.jpg";
var mentor = await service.BecomeMentorAsync(userId, request, ct);
Check("Application starts Pending and inactive", mentor.ApprovalStatus == "Pending" && !mentor.IsActive);
await Expect<NotFoundException>("Pending application is not public", () => service.GetByIdAsync(mentor.Id, ct));
await service.UpdateProfileAsync(userId, new UpdateMentorProfileRequest { Bio = request.Bio, Expertise = request.Expertise, IsActive = true }, ct);
Check("Producer cannot activate pending mentor via profile update", !repository.Item!.IsActive);
var rejected = await service.ReviewAsync(mentor.Id, adminId, false, "Please provide a clearer photo", ct);
Check("Rejection returns the saved decision", rejected.ApprovalStatus == "Rejected" && !rejected.IsActive && rejected.ReviewNote != null);
mentor = await service.BecomeMentorAsync(userId, request, ct);
Check("Rejected application can be resubmitted without duplicate profile", mentor.Id == rejected.Id && mentor.ApprovalStatus == "Pending" && mentor.ReviewedAt == null);
var approved = await service.ReviewAsync(mentor.Id, adminId, true, "Approved for teaching", ct);
Check("Approval activates mentor and saves admin identity", approved.IsActive && repository.Item!.ReviewedByUserId == adminId && approved.ReviewedAt != null);
var visible = await service.GetByIdAsync(mentor.Id, ct);
Check("Public profile excludes application proof and private review", visible.ProofImageUrl == "" && visible.ReviewNote == null);
Check("Owner retains proof for their application", (await service.GetMyProfileAsync(userId, ct)).ProofImageUrl == request.ProofImageUrl);
await Expect<ConflictException>("Reviewed application cannot be reviewed twice", () => service.ReviewAsync(mentor.Id, adminId, false, "Second review", ct));
var validator = new CreateCourseRequestValidator();
var course = new CreateCourseRequest { Title = "Weaving", Description = "Learn weaving", Category = "Textile", Price = 500, MaxApprentices = 10, DeliveryMode = "Both", Venue = "Dhaka" };
Check("Complete course offer is valid", validator.Validate(course).IsValid);
course.Price = -1;
Check("Negative fees are invalid", !validator.Validate(course).IsValid);
course.Price = 0; course.DaysPerWeek = 8;
Check("Impossible weekly schedule is invalid", !validator.Validate(course).IsValid);
course.DaysPerWeek = 2; course.Venue = null;
Check("Offline attendance requires venue", !validator.Validate(course).IsValid);
Console.WriteLine("ALL ACADEMY REGRESSIONS PASSED");

static void Check(string message, bool passed) { if (!passed) throw new Exception(message); Console.WriteLine("PASS " + message); }
static async Task Expect<T>(string message, Func<Task<MentorProfileDto>> action) where T : Exception
{ try { await action(); } catch (T) { Check(message, true); return; } throw new Exception(message); }

sealed class MentorFixture : IMentorRepository
{
    public MentorProfile? Item;
    public Task<MentorProfile?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Item?.Id == id ? Item : null);
    public Task<MentorProfile?> GetByUserIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Item?.UserId == id ? Item : null);
    public Task<(List<MentorProfile> Items, int TotalCount)> GetPagedAsync(bool activeOnly, int page, int size, CancellationToken ct)
    { var items = Item is null || (activeOnly && !Item.IsActive) ? new List<MentorProfile>() : new List<MentorProfile> { Item }; return Task.FromResult((items, items.Count)); }
    public Task AddAsync(MentorProfile item, CancellationToken ct) { Item = item; item.User = new User { Id = item.UserId, FullName = "QA Mentor" }; return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    public Task<MentorSkill?> GetSkillAsync(Guid mentor, Guid skill, CancellationToken ct) => throw new NotSupportedException();
    public Task AddSkillAsync(MentorSkill skill, CancellationToken ct) => throw new NotSupportedException();
    public void RemoveSkill(MentorSkill skill) => throw new NotSupportedException();
}
