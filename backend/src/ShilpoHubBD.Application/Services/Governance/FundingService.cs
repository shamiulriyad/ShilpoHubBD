using System.Text.RegularExpressions;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO funding pipeline: grant / loan / scholarship / equipment-support and
/// village / producer sponsorship programmes, their applications, reviews, approval decisions,
/// disbursement scheduling and (for loans) repayment tracking. Programme budget counters
/// (<c>AllocatedAmount</c>, <c>DisbursedAmount</c>) are maintained on write.
/// </summary>
public class FundingService : IFundingService
{
    private readonly IFundingRepository _repository;

    public FundingService(IFundingRepository repository)
    {
        _repository = repository;
    }

    // ==== Programs ====================================================

    public async Task<FundingProgramDto> CreateProgramAsync(
        Guid userId, CreateFundingProgramRequest request, CancellationToken cancellationToken)
    {
        var type = ParseEnum<FundingProgramType>(request.Type, "Invalid Type.");
        if (request.TotalBudget < 0)
        {
            throw new ConflictException("TotalBudget cannot be negative.");
        }

        ValidateAmountBand(request.MinAmountPerApplicant, request.MaxAmountPerApplicant);

        var now = DateTime.UtcNow;
        var isLoan = type == FundingProgramType.Loan;
        var program = new FundingProgram
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = await UniqueSlugAsync(request.Name, cancellationToken),
            Type = type,
            Status = FundingProgramStatus.Draft,
            Description = request.Description.Trim(),
            EligibilityCriteria = request.EligibilityCriteria?.Trim(),
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BDT" : request.Currency.Trim().ToUpperInvariant(),
            TotalBudget = request.TotalBudget,
            MinAmountPerApplicant = request.MinAmountPerApplicant,
            MaxAmountPerApplicant = request.MaxAmountPerApplicant,
            ApplicationOpensAt = ToUtc(request.ApplicationOpensAt),
            ApplicationClosesAt = ToUtc(request.ApplicationClosesAt),
            RequiresRepayment = isLoan || request.RequiresRepayment,
            InterestRatePercent = isLoan ? request.InterestRatePercent : null,
            RepaymentPeriodMonths = isLoan ? request.RepaymentPeriodMonths : null,
            ManagedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddProgramAsync(program, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetProgramByIdAsync(program.Id, cancellationToken))!.ToDto(0, 0);
    }

    public async Task<PagedResult<FundingProgramListItemDto>> GetProgramsAsync(
        FundingProgramQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetProgramsPagedAsync(query, cancellationToken);
        var counts = await _repository.GetApplicationCountsAsync(items.Select(p => p.Id), cancellationToken);

        return new PagedResult<FundingProgramListItemDto>
        {
            Items = items.Select(p => p.ToListItemDto(counts.GetValueOrDefault(p.Id))).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<FundingProgramDto> GetProgramByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var program = await LoadProgramAsync(id, cancellationToken);
        var (appCount, approvedCount) = await _repository.GetProgramCountsAsync(id, cancellationToken);
        return program.ToDto(appCount, approvedCount);
    }

    public async Task<FundingProgramDto> UpdateProgramAsync(
        Guid userId, Guid id, UpdateFundingProgramRequest request, CancellationToken cancellationToken)
    {
        var program = await LoadProgramAsync(id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            program.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            program.Status = ParseEnum<FundingProgramStatus>(request.Status, "Invalid Status.");
        }

        if (request.Description is not null)
        {
            program.Description = request.Description.Trim();
        }

        if (request.EligibilityCriteria is not null)
        {
            program.EligibilityCriteria = request.EligibilityCriteria.Trim();
        }

        if (request.TotalBudget.HasValue)
        {
            if (request.TotalBudget.Value < program.AllocatedAmount)
            {
                throw new ConflictException(
                    $"TotalBudget cannot be below the already-allocated amount ({program.AllocatedAmount:N0}).");
            }

            program.TotalBudget = request.TotalBudget.Value;
        }

        if (request.MinAmountPerApplicant.HasValue)
        {
            program.MinAmountPerApplicant = request.MinAmountPerApplicant;
        }

        if (request.MaxAmountPerApplicant.HasValue)
        {
            program.MaxAmountPerApplicant = request.MaxAmountPerApplicant;
        }

        ValidateAmountBand(program.MinAmountPerApplicant, program.MaxAmountPerApplicant);

        if (request.ApplicationOpensAt.HasValue)
        {
            program.ApplicationOpensAt = ToUtc(request.ApplicationOpensAt);
        }

        if (request.ApplicationClosesAt.HasValue)
        {
            program.ApplicationClosesAt = ToUtc(request.ApplicationClosesAt);
        }

        if (program.Type == FundingProgramType.Loan)
        {
            if (request.RequiresRepayment.HasValue)
            {
                program.RequiresRepayment = request.RequiresRepayment.Value;
            }

            if (request.InterestRatePercent.HasValue)
            {
                program.InterestRatePercent = request.InterestRatePercent;
            }

            if (request.RepaymentPeriodMonths.HasValue)
            {
                program.RepaymentPeriodMonths = request.RepaymentPeriodMonths;
            }
        }

        program.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        var (appCount, approvedCount) = await _repository.GetProgramCountsAsync(id, cancellationToken);
        return (await _repository.GetProgramByIdAsync(id, cancellationToken))!.ToDto(appCount, approvedCount);
    }

    public async Task DeleteProgramAsync(Guid id, CancellationToken cancellationToken)
    {
        var program = await LoadProgramAsync(id, cancellationToken);
        var (appCount, _) = await _repository.GetProgramCountsAsync(id, cancellationToken);
        if (appCount > 0)
        {
            throw new ConflictException("A programme with applications cannot be deleted; archive it instead.");
        }

        _repository.RemoveProgram(program);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ==== Applications ===============================================

    public async Task<FundingApplicationDto> CreateApplicationAsync(
        Guid userId, CreateFundingApplicationRequest request, CancellationToken cancellationToken)
    {
        var program = await LoadProgramAsync(request.FundingProgramId, cancellationToken);
        if (program.Status != FundingProgramStatus.Open)
        {
            throw new ConflictException("The programme is not open for applications.");
        }

        var now = DateTime.UtcNow;
        if (program.ApplicationClosesAt is { } closes && closes < now)
        {
            throw new ConflictException("The application window has closed.");
        }

        var applicantType = ParseEnum<FundingApplicantType>(request.ApplicantType, "Invalid ApplicantType.");

        if (request.RequestedAmount <= 0)
        {
            throw new ConflictException("RequestedAmount must be greater than zero.");
        }

        if (program.MinAmountPerApplicant is { } min && request.RequestedAmount < min)
        {
            throw new ConflictException($"RequestedAmount is below the programme minimum ({min:N0}).");
        }

        if (program.MaxAmountPerApplicant is { } max && request.RequestedAmount > max)
        {
            throw new ConflictException($"RequestedAmount exceeds the programme maximum ({max:N0}).");
        }

        if (request.ApplicantUserId is { } uid && !await _repository.UserExistsAsync(uid, cancellationToken))
        {
            throw new NotFoundException("Applicant user not found.");
        }

        if (request.ApplicantVillageId is { } vid && !await _repository.VillageExistsAsync(vid, cancellationToken))
        {
            throw new NotFoundException("Applicant village not found.");
        }

        var application = new FundingApplication
        {
            Id = Guid.NewGuid(),
            FundingProgramId = program.Id,
            ReferenceCode = await UniqueApplicationReferenceAsync(now, cancellationToken),
            ApplicantType = applicantType,
            ApplicantUserId = request.ApplicantUserId,
            ApplicantVillageId = request.ApplicantVillageId,
            ApplicantLabel = request.ApplicantLabel.Trim(),
            Status = FundingApplicationStatus.Submitted,
            RequestedAmount = request.RequestedAmount,
            Purpose = request.Purpose.Trim(),
            Justification = request.Justification?.Trim(),
            ContactName = request.ContactName?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            SubmittedAt = now,
            RepaymentStatus = LoanRepaymentStatus.NotApplicable,
            CreatedAt = now,
            UpdatedAt = now,
        };
        AddEvent(application, FundingApplicationEventType.Submitted, userId, now, "Application submitted.");

        await _repository.AddApplicationAsync(application, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetApplicationByIdAsync(application.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<FundingApplicationListItemDto>> GetApplicationsAsync(
        FundingApplicationQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetApplicationsPagedAsync(query, cancellationToken);

        return new PagedResult<FundingApplicationListItemDto>
        {
            Items = items.Select(a => a.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<FundingApplicationDto> GetApplicationByIdAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadApplicationAsync(id, cancellationToken)).ToDto();

    public async Task<FundingApplicationDto> AddReviewAsync(
        Guid userId, Guid id, SubmitFundingReviewRequest request, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        if (application.Status is not (FundingApplicationStatus.Submitted or FundingApplicationStatus.UnderReview))
        {
            throw new ConflictException($"An application in status {application.Status} cannot be reviewed.");
        }

        var decision = ParseEnum<FundingReviewDecision>(request.Decision, "Invalid Decision.");
        if (request.Score is < 0 or > 100)
        {
            throw new ConflictException("Score must be between 0 and 100.");
        }

        if (request.RecommendedAmount is < 0)
        {
            throw new ConflictException("RecommendedAmount cannot be negative.");
        }

        var now = DateTime.UtcNow;
        var from = application.Status;
        application.Status = FundingApplicationStatus.UnderReview;
        application.UpdatedAt = now;
        application.Reviews.Add(new FundingApplicationReview
        {
            Id = Guid.NewGuid(),
            FundingApplicationId = application.Id,
            ReviewerUserId = userId,
            Decision = decision,
            Score = request.Score,
            RecommendedAmount = request.RecommendedAmount,
            Comments = request.Comments?.Trim(),
            CreatedAt = now,
        });
        AddEvent(application, FundingApplicationEventType.Reviewed, userId, now,
            $"Review recorded: {decision}.", from, application.Status);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<FundingApplicationDto> DecideAsync(
        Guid userId, Guid id, DecideFundingApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        if (application.Status is not (FundingApplicationStatus.Submitted or FundingApplicationStatus.UnderReview))
        {
            throw new ConflictException($"An application in status {application.Status} cannot be decided.");
        }

        var outcome = ParseEnum<FundingApplicationStatus>(request.Outcome, "Outcome must be Approved or Rejected.");
        if (outcome is not (FundingApplicationStatus.Approved or FundingApplicationStatus.Rejected))
        {
            throw new ConflictException("Outcome must be Approved or Rejected.");
        }

        var program = application.Program;
        var now = DateTime.UtcNow;
        var from = application.Status;

        if (outcome == FundingApplicationStatus.Rejected)
        {
            application.Status = FundingApplicationStatus.Rejected;
            application.DecisionAt = now;
            application.DecisionByUserId = userId;
            application.DecisionNotes = request.Notes?.Trim();
            AddEvent(application, FundingApplicationEventType.Rejected, userId, now,
                request.Notes?.Trim() ?? "Application rejected.", from, application.Status);
        }
        else
        {
            var amount = request.ApprovedAmount
                ?? throw new ConflictException("ApprovedAmount is required when approving.");
            if (amount <= 0)
            {
                throw new ConflictException("ApprovedAmount must be greater than zero.");
            }

            if (program.MinAmountPerApplicant is { } min && amount < min)
            {
                throw new ConflictException($"ApprovedAmount is below the programme minimum ({min:N0}).");
            }

            if (program.MaxAmountPerApplicant is { } max && amount > max)
            {
                throw new ConflictException($"ApprovedAmount exceeds the programme maximum ({max:N0}).");
            }

            var remaining = program.TotalBudget - program.AllocatedAmount;
            if (amount > remaining)
            {
                throw new ConflictException(
                    $"ApprovedAmount exceeds the programme's remaining budget ({remaining:N0}).");
            }

            application.Status = FundingApplicationStatus.Approved;
            application.ApprovedAmount = amount;
            application.DecisionAt = now;
            application.DecisionByUserId = userId;
            application.DecisionNotes = request.Notes?.Trim();

            program.AllocatedAmount += amount;
            program.UpdatedAt = now;

            if (program.RequiresRepayment)
            {
                var interest = program.InterestRatePercent is { } rate and > 0
                    ? amount * rate / 100m
                    : 0m;
                application.RepaymentStatus = LoanRepaymentStatus.Pending;
                application.OutstandingBalance = amount + interest;
            }

            AddEvent(application, FundingApplicationEventType.Approved, userId, now,
                $"Approved for {amount:N0} {program.Currency}.", from, application.Status);
        }

        application.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<FundingApplicationDto> WithdrawAsync(
        Guid userId, Guid id, WithdrawFundingApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        if (application.Status is FundingApplicationStatus.Completed or FundingApplicationStatus.Rejected
            or FundingApplicationStatus.Withdrawn)
        {
            throw new ConflictException($"An application in status {application.Status} cannot be withdrawn.");
        }

        var now = DateTime.UtcNow;
        var from = application.Status;

        if (application.Status is FundingApplicationStatus.Approved or FundingApplicationStatus.Disbursing
            && application.ApprovedAmount is { } approved)
        {
            var paid = application.Disbursements
                .Where(d => d.Status == FundingDisbursementStatus.Paid)
                .Sum(d => d.Amount);
            if (paid > 0)
            {
                throw new ConflictException("Funds have already been disbursed; this application cannot be withdrawn.");
            }

            application.Program.AllocatedAmount -= approved;
            application.Program.UpdatedAt = now;
            foreach (var d in application.Disbursements.Where(d => d.Status == FundingDisbursementStatus.Scheduled))
            {
                d.Status = FundingDisbursementStatus.Cancelled;
                d.UpdatedAt = now;
            }

            application.ApprovedAmount = null;
            application.RepaymentStatus = LoanRepaymentStatus.NotApplicable;
            application.OutstandingBalance = 0;
        }

        application.Status = FundingApplicationStatus.Withdrawn;
        application.UpdatedAt = now;
        AddEvent(application, FundingApplicationEventType.Withdrawn, userId, now,
            request.Reason?.Trim() ?? "Application withdrawn.", from, application.Status);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<FundingApplicationDto> AddNoteAsync(
        Guid userId, Guid id, AddFundingApplicationNoteRequest request, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        var now = DateTime.UtcNow;
        application.UpdatedAt = now;
        AddEvent(application, FundingApplicationEventType.Note, userId, now, request.Note.Trim());

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<FundingApplicationDto> ScheduleDisbursementAsync(
        Guid userId, Guid id, ScheduleFundingDisbursementRequest request, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        if (application.Status is not (FundingApplicationStatus.Approved or FundingApplicationStatus.Disbursing))
        {
            throw new ConflictException("Disbursements can only be scheduled for an approved application.");
        }

        var method = ParseEnum<FundingDisbursementMethod>(request.Method, "Invalid Method.");
        if (request.Amount <= 0)
        {
            throw new ConflictException("Amount must be greater than zero.");
        }

        var committed = application.Disbursements
            .Where(d => d.Status is FundingDisbursementStatus.Scheduled or FundingDisbursementStatus.Paid)
            .Sum(d => d.Amount);
        if (application.ApprovedAmount is { } approved && committed + request.Amount > approved)
        {
            throw new ConflictException(
                $"Scheduled + paid disbursements would exceed the approved amount ({approved:N0}).");
        }

        var now = DateTime.UtcNow;
        var from = application.Status;
        application.Disbursements.Add(new FundingDisbursement
        {
            Id = Guid.NewGuid(),
            FundingApplicationId = application.Id,
            Amount = request.Amount,
            Method = method,
            Status = FundingDisbursementStatus.Scheduled,
            ScheduledFor = ToUtc(request.ScheduledFor) ?? now,
            Reference = request.Reference?.Trim(),
            Notes = request.Notes?.Trim(),
            RecordedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        });
        application.Status = FundingApplicationStatus.Disbursing;
        application.UpdatedAt = now;
        AddEvent(application, FundingApplicationEventType.DisbursementScheduled, userId, now,
            $"Scheduled {request.Amount:N0} via {method}.", from, application.Status);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<FundingApplicationDto> UpdateDisbursementStatusAsync(
        Guid userId, Guid id, Guid disbursementId, UpdateFundingDisbursementStatusRequest request,
        CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        var disbursement = application.Disbursements.FirstOrDefault(d => d.Id == disbursementId)
            ?? throw new NotFoundException("Disbursement not found on this application.");

        var newStatus = ParseEnum<FundingDisbursementStatus>(request.Status, "Invalid Status.");
        var now = DateTime.UtcNow;

        if (disbursement.Status == FundingDisbursementStatus.Paid && newStatus != FundingDisbursementStatus.Paid)
        {
            throw new ConflictException("A paid disbursement cannot change status.");
        }

        if (newStatus == disbursement.Status)
        {
            throw new ConflictException($"Disbursement is already {newStatus}.");
        }

        disbursement.Status = newStatus;
        disbursement.UpdatedAt = now;
        if (request.Reference is not null)
        {
            disbursement.Reference = request.Reference.Trim();
        }

        if (request.Notes is not null)
        {
            disbursement.Notes = request.Notes.Trim();
        }

        if (newStatus == FundingDisbursementStatus.Paid)
        {
            disbursement.PaidAt = ToUtc(request.PaidAt) ?? now;
            application.Program.DisbursedAmount += disbursement.Amount;
            application.Program.UpdatedAt = now;

            var totalPaid = application.Disbursements
                .Where(d => d.Status == FundingDisbursementStatus.Paid)
                .Sum(d => d.Amount);
            if (application.ApprovedAmount is { } approved && totalPaid >= approved
                && application.RepaymentStatus is LoanRepaymentStatus.NotApplicable)
            {
                application.Status = FundingApplicationStatus.Completed;
            }
            else if (application.RepaymentStatus == LoanRepaymentStatus.Pending)
            {
                application.RepaymentStatus = LoanRepaymentStatus.InRepayment;
            }

            AddEvent(application, FundingApplicationEventType.DisbursementPaid, userId, now,
                $"Paid {disbursement.Amount:N0}.");
        }
        else
        {
            AddEvent(application, FundingApplicationEventType.StatusChanged, userId, now,
                $"Disbursement marked {newStatus}.");
        }

        application.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task<FundingApplicationDto> RecordRepaymentAsync(
        Guid userId, Guid id, RecordLoanRepaymentRequest request, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        if (!application.Program.RequiresRepayment
            || application.RepaymentStatus is LoanRepaymentStatus.NotApplicable or LoanRepaymentStatus.Repaid)
        {
            throw new ConflictException("This application has no outstanding repayment.");
        }

        if (request.Amount <= 0)
        {
            throw new ConflictException("Amount must be greater than zero.");
        }

        if (request.Amount > application.OutstandingBalance)
        {
            throw new ConflictException(
                $"Amount exceeds the outstanding balance ({application.OutstandingBalance:N0}).");
        }

        var now = DateTime.UtcNow;
        application.OutstandingBalance -= request.Amount;
        application.TotalRepaid += request.Amount;
        application.NextRepaymentDueAt = ToUtc(request.NextDueAt);
        application.RepaymentStatus = application.OutstandingBalance <= 0
            ? LoanRepaymentStatus.Repaid
            : LoanRepaymentStatus.InRepayment;

        if (application.RepaymentStatus == LoanRepaymentStatus.Repaid)
        {
            application.Status = FundingApplicationStatus.Completed;
            application.NextRepaymentDueAt = null;
        }

        application.UpdatedAt = now;
        AddEvent(application, FundingApplicationEventType.RepaymentRecorded, userId, now,
            $"Repayment of {request.Amount:N0} recorded"
            + (request.Notes is null ? "." : $": {request.Notes.Trim()}"));

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetApplicationByIdAsync(id, cancellationToken))!.ToDto();
    }

    public async Task DeleteApplicationAsync(Guid id, CancellationToken cancellationToken)
    {
        var application = await LoadApplicationAsync(id, cancellationToken);
        if (application.Status is FundingApplicationStatus.Approved or FundingApplicationStatus.Disbursing
            or FundingApplicationStatus.Completed)
        {
            throw new ConflictException(
                "An application that has been approved or disbursed cannot be deleted; withdraw it instead.");
        }

        _repository.RemoveApplication(application);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ==== helpers ====================================================

    private async Task<FundingProgram> LoadProgramAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetProgramByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Funding programme not found.");

    private async Task<FundingApplication> LoadApplicationAsync(Guid id, CancellationToken cancellationToken)
        => await _repository.GetApplicationByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Funding application not found.");

    private static void AddEvent(
        FundingApplication application, FundingApplicationEventType type, Guid actorUserId, DateTime at,
        string? note, FundingApplicationStatus? from = null, FundingApplicationStatus? to = null)
        => application.Events.Add(new FundingApplicationEvent
        {
            Id = Guid.NewGuid(),
            FundingApplicationId = application.Id,
            Type = type,
            Note = note,
            FromStatus = from,
            ToStatus = to,
            ActorUserId = actorUserId,
            CreatedAt = at,
        });

    private static void ValidateAmountBand(decimal? min, decimal? max)
    {
        if (min is < 0 || max is < 0)
        {
            throw new ConflictException("Amount limits cannot be negative.");
        }

        if (min.HasValue && max.HasValue && min > max)
        {
            throw new ConflictException("MinAmountPerApplicant cannot exceed MaxAmountPerApplicant.");
        }
    }

    private async Task<string> UniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(name);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "programme";
        }

        var slug = baseSlug;
        var suffix = 2;
        while (await _repository.ProgramSlugExistsAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        lower = Regex.Replace(lower, "[^a-z0-9]+", "-").Trim('-');
        return lower.Length > 140 ? lower[..140] : lower;
    }

    private async Task<string> UniqueApplicationReferenceAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 6; attempt++)
        {
            var candidate = $"FND-{now:yyyyMM}-{Random.Shared.Next(0, 100000):D5}";
            if (!await _repository.ApplicationReferenceExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return $"FND-{now:yyyyMM}-{Guid.NewGuid():N}"[..20];
    }

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
