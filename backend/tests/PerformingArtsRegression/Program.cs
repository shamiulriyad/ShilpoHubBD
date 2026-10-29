// Regression checks for the Performing Arts heritage module: Super Admin-only CRUD + editorial workflow
// (Draft -> InReview -> Published -> Archived -> Draft), publish gated on cited sources, public endpoints that only
// ever expose Published records, bilingual search and filters, bilingual/media/source validation, and the AI Heritage
// Assistant answering performing-arts questions from Published records only (with sources and detail-page links)
// while everything else still goes to the RAG provider. Controller authorization is checked by reflection over the
// real attributes. No database, no network, no web host -- in-memory fakes only. Run with `dotnet run`.
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Application.DTOs.PerformingArts;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.HeritageAssistant;
using ShilpoHubBD.Application.Services.PerformingArts;
using ShilpoHubBD.Application.Validators.PerformingArts;
using ShilpoHubBD.Data;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.PerformingArts;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }
async Task<bool> Throws<TException>(Func<Task> action) where TException : Exception
{
    try { await action(); return false; } catch (TException) { return true; }
}

var ct = CancellationToken.None;
var adminId = Guid.NewGuid();
var thakurgaon = new District { Id = Guid.NewGuid(), Name = "Thakurgaon", Division = "Rangpur" };
var kushtia = new District { Id = Guid.NewGuid(), Name = "Kushtia", Division = "Khulna" };
var repo = new FakePerformingArtRepository(thakurgaon, kushtia);
var service = new PerformingArtService(repo);

SavePerformingArtSourceRequest ChibSource(string title) => new()
{
    SourceType = PerformingArtSourceType.OfficialInventory, Title = $"Cultural Heritage Inventory of Bangladesh — {title}",
    Publisher = "Ministry of Cultural Affairs, Bangladesh", Url = "https://heritagehub.gov.bd/HeritageInfoes/Details/3184",
};

SavePerformingArtRequest Request(string slug, string titleEn, string? titleBn, PerformingArtCategory category, Guid? districtId,
    string summary, string? communities = null, bool withSource = true, string? summaryBn = null) => new()
{
    Slug = slug, TitleEn = titleEn, TitleBn = titleBn, Category = category, DistrictId = districtId, SummaryEn = summary,
    SummaryBn = summaryBn, Communities = communities, Division = "Dhaka", // Division is overridden by the district
    Sources = withSource ? new List<SavePerformingArtSourceRequest> { ChibSource(titleEn) } : new(),
};

// ===================== Authorization (reflection over the real controllers) =====================
var adminType = typeof(AdminPerformingArtsController);
var adminAuth = adminType.GetCustomAttribute<AuthorizeAttribute>();
Check("Admin controller is gated to SuperAdmin at class level", adminAuth?.Roles == RoleNames.SuperAdmin);
var adminActions = adminType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
Check("No admin action opts out with [AllowAnonymous]", adminActions.All(m => m.GetCustomAttribute<AllowAnonymousAttribute>() is null));
Check("No admin action overrides the role with a broader [Authorize]",
    adminActions.All(m => m.GetCustomAttributes<AuthorizeAttribute>().All(a => a.Roles is null || a.Roles == RoleNames.SuperAdmin)));
var workflowRoutes = adminActions.SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>()).Select(a => a.Template).ToHashSet();
Check("Admin exposes create/read/update/delete + submit/publish/return/archive/restore",
    new[] { "{id:guid}/submit", "{id:guid}/publish", "{id:guid}/return", "{id:guid}/archive", "{id:guid}/restore", "{id:guid}" }.All(workflowRoutes.Contains));
var publicType = typeof(PerformingArtsController);
Check("Public controller allows anonymous visitors", publicType.GetCustomAttribute<AllowAnonymousAttribute>() is not null);
var publicVerbs = publicType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
    .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>()).SelectMany(a => a.HttpMethods).ToHashSet();
Check("Public controller is read-only (GET only)", publicVerbs.SetEquals(new[] { "GET" }));

// ===================== Create / update / slug rules =====================
var dhamer = await service.CreateAsync(Request("dhamer-gaan", "Dhamer Gaan", "ধামের গান", PerformingArtCategory.FolkTheatre, thakurgaon.Id,
    "A rural narrative theatre form of north-west Bangladesh in which male performers play all the roles.", "Barman, Rajbangshi"), adminId, ct);
Check("Create starts as Draft", dhamer.Status == PerformingArtStatus.Draft);
Check("Create records the author", repo.Records.Single(r => r.Id == dhamer.Id).CreatedByUserId == adminId);
Check("Division is derived from the district, not the request", dhamer.Division == "Rangpur" && dhamer.DistrictName == "Thakurgaon");
Check("Duplicate slug is rejected", await Throws<ConflictException>(() =>
    service.CreateAsync(Request("Dhamer-Gaan", "Copy", null, PerformingArtCategory.FolkTheatre, null, "x"), adminId, ct)));
Check("Unknown district is rejected", await Throws<NotFoundException>(() =>
    service.CreateAsync(Request("ghost", "Ghost", null, PerformingArtCategory.FolkMusic, Guid.NewGuid(), "x"), adminId, ct)));

var update = Request("dhamer-gaan", "Dhamer Gaan", "ধামের গান", PerformingArtCategory.FolkTheatre, thakurgaon.Id,
    "A rural narrative theatre form of north-west Bangladesh in which male performers play all the roles.", "Barman, Rajbangshi, Koch");
update.Media.Add(new SavePerformingArtMediaRequest { MediaType = PerformingArtMediaType.Image, Url = "/uploads/images/dhamer.webp", Credit = "Test credit" });
update.Sources.Add(new SavePerformingArtSourceRequest { SourceType = PerformingArtSourceType.Book, Title = "Second source" });
var updated = await service.UpdateAsync(dhamer.Id, update, ct);
Check("Update replaces sources and media in order", updated.Sources.Select(s => s.Title).SequenceEqual(new[] { "Cultural Heritage Inventory of Bangladesh — Dhamer Gaan", "Second source" }) && updated.Media.Count == 1);
var listItem = (await service.SearchAdminAsync(new PerformingArtQueryParameters { Search = "dhamer" }, ct)).Items.Single();
Check("List item exposes the first image as cover with its credit", listItem.CoverImageUrl == "/uploads/images/dhamer.webp" && listItem.CoverImageCredit == "Test credit");

// ===================== Workflow =====================
var unsourced = await service.CreateAsync(Request("unsourced", "Unsourced Song", null, PerformingArtCategory.FolkMusic, null, "No citation yet.", withSource: false), adminId, ct);
Check("Submitting without any source is blocked", await Throws<ConflictException>(() => service.SubmitForReviewAsync(unsourced.Id, adminId, null, ct)));
Check("Publishing straight from Draft is blocked (must be reviewed)", await Throws<ConflictException>(() => service.PublishAsync(dhamer.Id, adminId, null, ct)));
Check("Public detail hides a Draft", await Throws<NotFoundException>(() => service.GetPublishedBySlugAsync("dhamer-gaan", ct)));

var inReview = await service.SubmitForReviewAsync(dhamer.Id, adminId, null, ct);
Check("Submit moves Draft -> InReview and stamps SubmittedAt", inReview.Status == PerformingArtStatus.InReview && inReview.SubmittedAt != null);
Check("Public detail hides an InReview record", await Throws<NotFoundException>(() => service.GetPublishedBySlugAsync("dhamer-gaan", ct)));
Check("Returning to draft without a note is a validation error", await Throws<ValidationException>(() => service.ReturnToDraftAsync(dhamer.Id, adminId, " ", ct)));
Check("An InReview record cannot drop all its sources", await Throws<ConflictException>(() =>
    service.UpdateAsync(dhamer.Id, Request("dhamer-gaan", "Dhamer Gaan", null, PerformingArtCategory.FolkTheatre, thakurgaon.Id, "x", withSource: false), ct)));
Check("Deleting an InReview record is blocked", await Throws<ConflictException>(() => service.DeleteAsync(dhamer.Id, ct)));

var published = await service.PublishAsync(dhamer.Id, adminId, "Checked against CHIB", ct);
Check("Publish moves InReview -> Published, records reviewer and time", published.Status == PerformingArtStatus.Published && published.PublishedAt != null
    && repo.Records.Single(r => r.Id == dhamer.Id).ReviewedByUserId == adminId);
var detail = await service.GetPublishedBySlugAsync("DHAMER-GAAN", ct);
Check("Public detail serves a Published record (slug is case-insensitive)", detail.Id == dhamer.Id);
Check("Public detail carries source attribution", detail.Sources.Count == 2 && detail.Sources[0].Url == "https://heritagehub.gov.bd/HeritageInfoes/Details/3184");
Check("Public detail carries bilingual titles", detail.TitleEn == "Dhamer Gaan" && detail.TitleBn == "ধামের গান");

var ektaraRequest = Request("ektara", "Ektara", "একতারা", PerformingArtCategory.FolkMusic, kushtia.Id,
    "A traditional instrument of the Baul, used to perform Baul songs.", "Baul", summaryBn: "বাউলদের ঐতিহ্যবাহী বাদ্যযন্ত্র।");
ektaraRequest.Instruments = "Ektara";
ektaraRequest.IsFeatured = true;
var ektara = await service.CreateAsync(ektaraRequest, adminId, ct);
await service.SubmitForReviewAsync(ektara.Id, adminId, null, ct);
await service.PublishAsync(ektara.Id, adminId, null, ct);

var draftDance = await service.CreateAsync(Request("secret-dance", "Secret Dance", null, PerformingArtCategory.TraditionalDance, kushtia.Id, "Unreviewed draft."), adminId, ct);

// ===================== Public search & filters =====================
async Task<List<string>> Slugs(PerformingArtQueryParameters q) => (await service.SearchPublishedAsync(q, ct)).Items.Select(i => i.Slug).ToList();
var all = await Slugs(new PerformingArtQueryParameters());
Check("Public list returns only Published records", all.Count == 2 && !all.Contains("secret-dance") && !all.Contains("unsourced"));
Check("Featured records sort first", all[0] == "ektara");
Check("Search by English title (case-insensitive)", (await Slugs(new() { Search = "DHAMER" })).SequenceEqual(new[] { "dhamer-gaan" }));
Check("Search by Bangla title", (await Slugs(new() { Search = "একতারা" })).SequenceEqual(new[] { "ektara" }));
Check("Search by Bangla summary text", (await Slugs(new() { Search = "বাউলদের" })).SequenceEqual(new[] { "ektara" }));
Check("Search by community", (await Slugs(new() { Search = "rajbangshi" })).SequenceEqual(new[] { "dhamer-gaan" }));
Check("Search by district name", (await Slugs(new() { Search = "kushtia" })).SequenceEqual(new[] { "ektara" }));
Check("Search never surfaces an unpublished match", (await Slugs(new() { Search = "secret" })).Count == 0);
Check("Category filter", (await Slugs(new() { Category = PerformingArtCategory.FolkTheatre })).SequenceEqual(new[] { "dhamer-gaan" }));
Check("Division filter", (await Slugs(new() { Division = "Khulna" })).SequenceEqual(new[] { "ektara" }));
Check("District filter", (await Slugs(new() { DistrictId = thakurgaon.Id })).SequenceEqual(new[] { "dhamer-gaan" }));
Check("Status query parameter cannot widen the public list", (await Slugs(new() { Status = PerformingArtStatus.Draft })).Count == 2);
Check("Admin list can filter by status", (await service.SearchAdminAsync(new() { Status = PerformingArtStatus.Draft }, ct)).Items.Select(i => i.Slug).OrderBy(s => s)
    .SequenceEqual(new[] { "secret-dance", "unsourced" }));
var paged = await service.SearchPublishedAsync(new() { Page = 2, PageSize = 1 }, ct);
Check("Paging returns the requested slice and total", paged.TotalCount == 2 && paged.Items.Count == 1 && paged.TotalPages == 2);
Check("Page size is capped", (await service.SearchPublishedAsync(new() { PageSize = 5000 }, ct)).PageSize == 50);
Check("Featured endpoint only returns featured Published records", (await service.GetFeaturedAsync(6, ct)).Select(i => i.Slug).SequenceEqual(new[] { "ektara" }));
var facets = await service.GetFacetsAsync(ct);
Check("Facets count Published records only", facets.Categories.Sum(c => c.Count) == 2 && facets.Districts.Count == 2
    && facets.Divisions.Single(d => d.Division == "Khulna").Count == 1 && facets.Categories.All(c => c.Category != PerformingArtCategory.TraditionalDance));

// ===================== Validation: bilingual content, media, sources =====================
var validator = new SavePerformingArtRequestValidator();
bool Valid(Action<SavePerformingArtRequest> tweak)
{
    var r = Request("valid-slug", "Valid", "বৈধ", PerformingArtCategory.FolkMusic, null, "Summary");
    r.Division = null;
    tweak(r);
    return validator.Validate(r).IsValid;
}
SavePerformingArtMediaRequest Media(PerformingArtMediaType t, string url, string credit = "Credit") => new() { MediaType = t, Url = url, Credit = credit };
Check("A well-formed bilingual request is valid", Valid(_ => { }));
Check("Bangla title written in Latin script is rejected", !Valid(r => r.TitleBn = "Dhamer Gaan"));
Check("Bangla summary written in Latin script is rejected", !Valid(r => r.SummaryBn = "English text"));
Check("Bangla summary in Bangla script is accepted", Valid(r => r.SummaryBn = "গ্রামীণ লোকনাট্য।"));
Check("English title is required", !Valid(r => r.TitleEn = ""));
Check("English summary is required", !Valid(r => r.SummaryEn = " "));
Check("Unknown division is rejected", !Valid(r => r.Division = "Chittagong Hill"));
Check("Known division is accepted", Valid(r => r.Division = "Chattogram"));
Check("Uppercase / spaced slug is rejected", !Valid(r => r.Slug = "Dhamer Gaan"));
Check("Reserved slug 'featured' is rejected", !Valid(r => r.Slug = "featured"));
Check("Image: https .jpg accepted", Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "https://example.org/a/photo.JPG"))));
Check("Image: platform upload .webp accepted", Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "/uploads/images/x.webp"))));
Check("Image: plain http rejected", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "http://example.org/photo.jpg"))));
Check("Image: javascript: URL rejected", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "javascript:alert(1)"))));
Check("Image: path traversal in upload path rejected", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "/uploads/../appsettings.jpg"))));
Check("Image: an .mp3 file is rejected as an image", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "https://example.org/song.mp3"))));
Check("Audio: https .mp3 accepted", Valid(r => r.Media.Add(Media(PerformingArtMediaType.Audio, "https://example.org/song.mp3"))));
Check("Audio: a .jpg is rejected as audio", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Audio, "https://example.org/photo.jpg"))));
Check("Video: YouTube link accepted", Valid(r => r.Media.Add(Media(PerformingArtMediaType.Video, "https://www.youtube.com/watch?v=abc123"))));
Check("Video: .mp4 accepted", Valid(r => r.Media.Add(Media(PerformingArtMediaType.Video, "https://example.org/clip.mp4"))));
Check("Video: unknown host without a video file rejected", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Video, "https://example.org/watch"))));
Check("Media without a credit is rejected", !Valid(r => r.Media.Add(Media(PerformingArtMediaType.Image, "https://example.org/a.png", ""))));
Check("Bangla caption in Latin script is rejected", !Valid(r => r.Media.Add(new() { MediaType = PerformingArtMediaType.Image, Url = "https://example.org/a.png", Credit = "c", CaptionBn = "caption" })));
Check("Source without a title is rejected", !Valid(r => r.Sources.Add(new() { Title = "" })));
Check("Source with a non-web URL is rejected", !Valid(r => r.Sources.Add(new() { Title = "t", Url = "ftp://example.org/x" })));
Check("More than 20 sources is rejected", !Valid(r => r.Sources.AddRange(Enumerable.Range(0, 21).Select(i => new SavePerformingArtSourceRequest { Title = $"s{i}" }))));

// ===================== AI Heritage Assistant retrieval =====================
var rag = new FakeRagProvider();
var assistant = new HeritageAssistantService(new FakeDistrictRepository(), new FakeFestivalRepository(), new FakeUnescoRepository(), repo, rag, NullLogger<HeritageAssistantService>.Instance);
async Task<HeritageAssistantAnswerDto> Ask(string q) => await assistant.AskAsync(new AskHeritageAssistantRequest { Question = q }, ct);

var en = await Ask("What is Dhamer Gaan?");
Check("Named record: answered from performing-arts records, not RAG", en.Category == PerformingArtComposerCategory() && rag.Calls == 0);
Check("Named record: answer repeats the stored summary", en.Answer.Contains("male performers play all the roles"));
Check("Named record: answer cites the record's sources", en.Sources.Any(s => s.StartsWith("Cultural Heritage Inventory of Bangladesh — Dhamer Gaan")));
Check("Named record: links to the detail page", en.Links.Any(l => l.Url == "/explore/performing-arts/dhamer-gaan"));
Check("Named record: lists communities from the record", en.Answer.Contains("Barman, Rajbangshi, Koch"));

var bnNoTranslation = await Ask("ধামের গান সম্পর্কে বলুন");
Check("Bangla question matches the Bangla title", bnNoTranslation.Links.Any(l => l.Url.EndsWith("/dhamer-gaan")));
Check("Bangla question without a stored Bangla summary says so and falls back to English",
    bnNoTranslation.Answer.Contains("বাংলা বিবরণ এখনও যোগ করা হয়নি") && bnNoTranslation.Answer.Contains("male performers"));
var bnTranslated = await Ask("একতারার ইতিহাস কী?");
Check("Bangla question uses the stored Bangla summary (inflected Bangla title matches)", bnTranslated.Answer.Contains("বাউলদের ঐতিহ্যবাহী বাদ্যযন্ত্র") && !bnTranslated.Answer.Contains("এখনও যোগ করা হয়নি"));

var byCategory = await Ask("Which folk theatre traditions are there in Rangpur?");
Check("Category + place question lists matching records", byCategory.Category == PerformingArtComposerCategory() && byCategory.Links.Select(l => l.Url).SequenceEqual(new[] { "/explore/performing-arts/dhamer-gaan" }));

var unpublishedNamed = await Ask("Tell me about the Secret Dance");
Check("Draft records are never retrieved: falls through to RAG", rag.Calls == 1 && unpublishedNamed.Category == "rag");
var brokenAssistant = new HeritageAssistantService(new FakeDistrictRepository(), new FakeFestivalRepository(), new FakeUnescoRepository(),
    new ThrowingPerformingArtRepository(), rag, NullLogger<HeritageAssistantService>.Instance);
var degraded = await brokenAssistant.AskAsync(new AskHeritageAssistantRequest { Question = "What is Dhamer Gaan?" }, ct);
Check("If the performing-arts lookup fails (e.g. tables not migrated), the assistant still answers via RAG", degraded.Category == "rag" && rag.Calls == 2);
rag.Reset();
var craftQuestion = await Ask("How is Jamdani woven?");
Check("Unrelated craft question still goes to the RAG service", rag.Calls == 1 && craftQuestion.Category == "rag" && craftQuestion.Links.Count == 0);
Check("Word-boundary matching: 'Ektaras' plural still matches, 'ektarapur' does not",
    PerformingArtMatcher.Rank("ektaras of Kushtia", repo.Records).Any() && !PerformingArtMatcher.Rank("ektarapur", repo.Records).Any());

// ===================== The shared filter translates to PostgreSQL (offline: ToQueryString never connects) =====================
var npgsqlOptions = new DbContextOptionsBuilder<ShilpoHubDbContext>().UseNpgsql("Host=offline.invalid;Database=x;Username=x;Password=x").Options;
using (var db = new ShilpoHubDbContext(npgsqlOptions))
{
    var fullFilter = PerformingArtRules.BuildFilter(new PerformingArtQueryParameters
    {
        Search = "ধামের", Category = PerformingArtCategory.FolkTheatre, Division = "Rangpur", DistrictId = thakurgaon.Id, Featured = true,
    }, publishedOnly: true);
    string? sql = null;
    try { sql = db.PerformingArtRecords.Where(fullFilter).Include(r => r.District).OrderBy(r => r.TitleEn).ToQueryString(); } catch (Exception ex) { Console.WriteLine(ex.Message); }
    Check("Full search filter translates to SQL without client evaluation", sql != null);
    Check("Translated SQL filters on status, joins District for name search, and lower-cases", sql != null
        && sql.Contains("\"Status\"") && sql.Contains("\"Districts\"") && sql.Contains("lower(", StringComparison.OrdinalIgnoreCase));
}

// ===================== Archive / restore / delete =====================
var archived = await service.ArchiveAsync(ektara.Id, adminId, "Superseded", ct);
Check("Archive moves Published -> Archived and clears featured", archived.Status == PerformingArtStatus.Archived && !archived.IsFeatured && archived.ArchivedAt != null);
Check("Archived record disappears from the public list", !(await Slugs(new())).Contains("ektara"));
Check("Archived record disappears from the assistant", !(await Ask("What is an Ektara?")).Links.Any(l => l.Url.EndsWith("/ektara")));
Check("Archived records are read-only", await Throws<ConflictException>(() => service.UpdateAsync(ektara.Id, ektaraRequest, ct)));
Check("Archiving twice is blocked", await Throws<ConflictException>(() => service.ArchiveAsync(ektara.Id, adminId, null, ct)));
var restored = await service.RestoreAsync(ektara.Id, adminId, null, ct);
Check("Restore moves Archived -> Draft", restored.Status == PerformingArtStatus.Draft && restored.ArchivedAt == null && restored.PublishedAt == null);
await service.DeleteAsync(ektara.Id, ct);
Check("Draft record can be deleted", repo.Records.All(r => r.Id != ektara.Id));
Check("Published record cannot be deleted", await Throws<ConflictException>(() => service.DeleteAsync(dhamer.Id, ct)));
Check("Deleting an unknown record is NotFound", await Throws<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid(), ct)));
_ = draftDance;

Console.WriteLine(failures == 0 ? "\nALL CHECKS PASSED" : $"\n{failures} CHECK(S) FAILED");
return failures == 0 ? 0 : 1;

static string PerformingArtComposerCategory() => PerformingArtAnswerComposer.AnswerCategory;

// ===================== Fakes =====================
sealed class FakePerformingArtRepository : IPerformingArtRepository
{
    private readonly Dictionary<Guid, District> _districts;
    public List<PerformingArtRecord> Records { get; } = new();
    private readonly List<PerformingArtRecord> _pendingAdds = new();
    private readonly List<PerformingArtRecord> _pendingRemoves = new();

    public FakePerformingArtRepository(params District[] districts) => _districts = districts.ToDictionary(d => d.Id);

    public Task<(List<PerformingArtRecord> Items, int TotalCount)> SearchAsync(
        Expression<Func<PerformingArtRecord, bool>> filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var matches = Records.AsQueryable().Where(filter)
            .OrderByDescending(r => r.IsFeatured).ThenBy(r => r.DisplayOrder).ThenBy(r => r.TitleEn).ToList();
        return Task.FromResult((matches.Skip((page - 1) * pageSize).Take(pageSize).ToList(), matches.Count));
    }

    public Task<List<PerformingArtRecord>> ListAsync(Expression<Func<PerformingArtRecord, bool>> filter, CancellationToken cancellationToken)
        => Task.FromResult(Records.AsQueryable().Where(filter).OrderBy(r => r.DisplayOrder).ThenBy(r => r.TitleEn).ToList());

    public Task<PerformingArtRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Records.FirstOrDefault(r => r.Id == id));
    public Task<PerformingArtRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken) => Task.FromResult(Records.FirstOrDefault(r => r.Slug == slug));
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken)
        => Task.FromResult(Records.Concat(_pendingAdds).Any(r => r.Slug == slug && (exceptId == null || r.Id != exceptId)));
    public Task<District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken) => Task.FromResult(_districts.GetValueOrDefault(districtId));
    public Task AddAsync(PerformingArtRecord record, CancellationToken cancellationToken) { _pendingAdds.Add(record); return Task.CompletedTask; }
    public void Remove(PerformingArtRecord record) => _pendingRemoves.Add(record);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Records.AddRange(_pendingAdds);
        Records.RemoveAll(_pendingRemoves.Contains);
        _pendingAdds.Clear();
        _pendingRemoves.Clear();
        return Task.CompletedTask;
    }
}

sealed class ThrowingPerformingArtRepository : IPerformingArtRepository
{
    private static Exception Missing() => new InvalidOperationException("relation \"PerformingArtRecords\" does not exist");
    public Task<(List<PerformingArtRecord> Items, int TotalCount)> SearchAsync(Expression<Func<PerformingArtRecord, bool>> filter, int page, int pageSize, CancellationToken cancellationToken) => throw Missing();
    public Task<List<PerformingArtRecord>> ListAsync(Expression<Func<PerformingArtRecord, bool>> filter, CancellationToken cancellationToken) => throw Missing();
    public Task<PerformingArtRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw Missing();
    public Task<PerformingArtRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken) => throw Missing();
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken) => throw Missing();
    public Task<District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken) => throw Missing();
    public Task AddAsync(PerformingArtRecord record, CancellationToken cancellationToken) => throw Missing();
    public void Remove(PerformingArtRecord record) => throw Missing();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => throw Missing();
}

sealed class FakeRagProvider : IHeritageAssistantProvider
{
    public int Calls { get; private set; }
    public void Reset() => Calls = 0;
    public Task<HeritageAssistantAnswerDto> AnswerAsync(HeritageAssistantContext context, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(new HeritageAssistantAnswerDto { Answer = "rag answer", Category = "rag" });
    }
}

sealed class FakeDistrictRepository : IDistrictRepository
{
    public Task<List<District>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken) => Task.FromResult(new List<District>());
    public Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<District?>(null);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

sealed class FakeFestivalRepository : IHeritageFestivalRepository
{
    public Task<(List<HeritageFestival> Items, int TotalCount)> GetPagedAsync(HeritageFestivalQueryParameters query, CancellationToken cancellationToken)
        => Task.FromResult((new List<HeritageFestival>(), 0));
    public Task<HeritageFestival?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<HeritageFestival?>(null);
    public Task AddAsync(HeritageFestival festival, CancellationToken cancellationToken) => Task.CompletedTask;
    public void Remove(HeritageFestival festival) { }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

sealed class FakeUnescoRepository : IUnescoRecordRepository
{
    public Task<List<UnescoRecord>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken) => Task.FromResult(new List<UnescoRecord>());
    public Task<UnescoRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<UnescoRecord?>(null);
    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken) => Task.FromResult(false);
    public Task AddAsync(UnescoRecord record, CancellationToken cancellationToken) => Task.CompletedTask;
    public void Remove(UnescoRecord record) { }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
