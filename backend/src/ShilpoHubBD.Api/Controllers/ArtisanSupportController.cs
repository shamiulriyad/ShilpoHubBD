using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController, Authorize, Route("api/artisan-support")]
public class ArtisanSupportController(IArtisanSupportService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool Admin => User.IsInRole(RoleNames.SuperAdmin); private bool Government => User.IsInRole(RoleNames.GovernmentNGO);

    [Authorize(Roles=RoleNames.GovernmentNGO), HttpGet("organization/me")]
    public async Task<IActionResult> MyOrganization(CancellationToken ct) => Ok(await service.GetMyOrganizationAsync(UserId,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO), HttpPut("organization/me")]
    public async Task<IActionResult> SaveOrganization(UpsertSupportOrganizationRequest request,CancellationToken ct)=>Ok(await service.UpsertOrganizationAsync(UserId,request,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO), HttpPost("organization/document"), RequestSizeLimit(10_500_000)]
    public async Task<IActionResult> UploadOrganizationDocument(IFormFile file,[FromServices] IWebHostEnvironment env,CancellationToken ct)
    {
        var allowed=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"application/pdf","image/jpeg","image/png","image/webp"};
        if(file is null||file.Length==0||file.Length>10*1024*1024||!allowed.Contains(file.ContentType))return BadRequest(new{message="Upload a PDF, JPG, PNG or WebP file smaller than 10 MB."});
        var ext=Path.GetExtension(file.FileName);if(string.IsNullOrWhiteSpace(ext))ext=file.ContentType=="application/pdf"?".pdf":".jpg";
        var folder=Path.Combine(env.WebRootPath??Path.Combine(env.ContentRootPath,"wwwroot"),"uploads","organization-documents");Directory.CreateDirectory(folder);var name=$"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        await using var stream=System.IO.File.Create(Path.Combine(folder,name));await file.CopyToAsync(stream,ct);return Ok(new{url=$"/uploads/organization-documents/{name}"});
    }
    [Authorize(Roles=RoleNames.SuperAdmin), HttpGet("organizations")]
    public async Task<IActionResult> Organizations(CancellationToken ct)=>Ok(await service.GetOrganizationsAsync(ct));
    [Authorize(Roles=RoleNames.SuperAdmin), HttpPost("organizations/{id:guid}/review")]
    public async Task<IActionResult> ReviewOrganization(Guid id,ReviewSupportOrganizationRequest request,CancellationToken ct)=>Ok(await service.ReviewOrganizationAsync(UserId,id,request,ct));
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}"),HttpGet("artisans")]
    public async Task<IActionResult> Artisans(CancellationToken ct)=>Ok(await service.GetArtisansAsync(ct));
    [Authorize(Roles=RoleNames.SuperAdmin),HttpGet("organizations/options")]
    public async Task<IActionResult> OrganizationOptions(CancellationToken ct)=>Ok(await service.GetApprovedOrganizationsAsync(ct));
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin},{RoleNames.Producer}"),HttpGet("cases")]
    public async Task<IActionResult> Cases(CancellationToken ct)=>Ok(await service.GetCasesAsync(UserId,Admin,Government,ct));
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin},{RoleNames.Producer}"),HttpGet("cases/{id:guid}")]
    public async Task<IActionResult> Case(Guid id,CancellationToken ct)=>Ok(await service.GetCaseAsync(UserId,id,Admin,Government,ct));
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.Producer}"),HttpPost("cases")]
    public async Task<IActionResult> Create(CreateArtisanSupportCaseRequest request,CancellationToken ct){var result=await service.CreateCaseAsync(UserId,Government,request,ct);return CreatedAtAction(nameof(Case),new{id=result.Id},result);}
    [Authorize(Roles=RoleNames.SuperAdmin),HttpPost("cases/{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id,AssignArtisanSupportCaseRequest request,CancellationToken ct)=>Ok(await service.AssignAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO),HttpPost("cases/{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id,CancellationToken ct)=>Ok(await service.AcceptAsync(UserId,id,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO),HttpPost("cases/{id:guid}/inspection")]
    public async Task<IActionResult> Inspect(Guid id,RecordSupportInspectionRequest request,CancellationToken ct)=>Ok(await service.InspectAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO),HttpPost("cases/{id:guid}/support-plan")]
    public async Task<IActionResult> Plan(Guid id,RecordSupportPlanRequest request,CancellationToken ct)=>Ok(await service.PlanAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO),HttpPost("cases/{id:guid}/support-provided")]
    public async Task<IActionResult> Provided(Guid id,RecordSupportProvidedRequest request,CancellationToken ct)=>Ok(await service.RecordProvidedAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.Producer),HttpPost("cases/{id:guid}/confirmation")]
    public async Task<IActionResult> Confirm(Guid id,ConfirmArtisanSupportRequest request,CancellationToken ct)=>Ok(await service.ConfirmAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO),HttpPost("cases/{id:guid}/monitoring")]
    public async Task<IActionResult> Monitor(Guid id,AddSupportMonitoringRequest request,CancellationToken ct)=>Ok(await service.MonitorAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.GovernmentNGO),HttpPost("cases/{id:guid}/final-report")]
    public async Task<IActionResult> Report(Guid id,SubmitSupportReportRequest request,CancellationToken ct)=>Ok(await service.SubmitReportAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.SuperAdmin),HttpPost("cases/{id:guid}/review-report")]
    public async Task<IActionResult> ReviewReport(Guid id,ReviewSupportReportRequest request,CancellationToken ct)=>Ok(await service.ReviewReportAsync(UserId,id,request,ct));
    [Authorize(Roles=RoleNames.SuperAdmin),HttpPost("cases/{id:guid}/flag")]
    public async Task<IActionResult> Flag(Guid id,FlagSupportCaseRequest request,CancellationToken ct)=>Ok(await service.FlagAsync(UserId,id,request,ct));
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin},{RoleNames.Producer}"),HttpPost("cases/{id:guid}/evidence")]
    public async Task<IActionResult> Evidence(Guid id,CreateSupportEvidenceRequest request,CancellationToken ct)=>Ok(await service.AddEvidenceAsync(UserId,id,Admin,request,ct));
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin},{RoleNames.Producer}"),HttpPost("cases/{id:guid}/evidence-upload"),RequestSizeLimit(10_500_000)]
    public async Task<IActionResult> UploadEvidence(Guid id,IFormFile file,[FromForm]string stage,[FromForm]string? caption,[FromServices]IWebHostEnvironment env,CancellationToken ct)
    {
        var allowed=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"application/pdf","image/jpeg","image/png","image/webp"};
        if(file is null||file.Length==0||file.Length>10*1024*1024||!allowed.Contains(file.ContentType))return BadRequest(new{message="Upload a PDF, JPG, PNG or WebP file smaller than 10 MB."});
        var ext=Path.GetExtension(file.FileName);if(string.IsNullOrWhiteSpace(ext))ext=file.ContentType=="application/pdf"?".pdf":".jpg";
        var folder=Path.Combine(env.WebRootPath??Path.Combine(env.ContentRootPath,"wwwroot"),"uploads","artisan-support");Directory.CreateDirectory(folder);var name=$"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        await using(var stream=System.IO.File.Create(Path.Combine(folder,name)))await file.CopyToAsync(stream,ct);
        return Ok(await service.AddEvidenceAsync(UserId,id,Admin,new CreateSupportEvidenceRequest(stage,$"/uploads/artisan-support/{name}",file.FileName,file.ContentType,caption),ct));
    }
    [Authorize(Roles=$"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin},{RoleNames.Producer}"),HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)=>Ok(await service.DashboardAsync(UserId,Admin,Government,ct));
}
