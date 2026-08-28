using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class CreateFundingProgramRequestValidator : AbstractValidator<CreateFundingProgramRequest>
{
    public CreateFundingProgramRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Type).NotEmpty().Must(BeEnum<FundingProgramType>).WithMessage("Invalid Type.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.EligibilityCriteria).MaximumLength(4000);
        RuleFor(x => x.Currency).MaximumLength(8);
        RuleFor(x => x.TotalBudget).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinAmountPerApplicant).GreaterThanOrEqualTo(0).When(x => x.MinAmountPerApplicant.HasValue);
        RuleFor(x => x.MaxAmountPerApplicant).GreaterThanOrEqualTo(0).When(x => x.MaxAmountPerApplicant.HasValue);
        RuleFor(x => x.InterestRatePercent).InclusiveBetween(0, 100).When(x => x.InterestRatePercent.HasValue);
        RuleFor(x => x.RepaymentPeriodMonths).InclusiveBetween(1, 600).When(x => x.RepaymentPeriodMonths.HasValue);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class UpdateFundingProgramRequestValidator : AbstractValidator<UpdateFundingProgramRequest>
{
    public UpdateFundingProgramRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(160);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.EligibilityCriteria).MaximumLength(4000);
        RuleFor(x => x.Status).Must(v => Enum.TryParse<FundingProgramStatus>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: Draft, Open, Closed, Archived.");
        RuleFor(x => x.TotalBudget).GreaterThanOrEqualTo(0).When(x => x.TotalBudget.HasValue);
        RuleFor(x => x.InterestRatePercent).InclusiveBetween(0, 100).When(x => x.InterestRatePercent.HasValue);
        RuleFor(x => x.RepaymentPeriodMonths).InclusiveBetween(1, 600).When(x => x.RepaymentPeriodMonths.HasValue);
    }
}

public class CreateFundingApplicationRequestValidator : AbstractValidator<CreateFundingApplicationRequest>
{
    public CreateFundingApplicationRequestValidator()
    {
        RuleFor(x => x.FundingProgramId).NotEmpty();
        RuleFor(x => x.ApplicantType).NotEmpty().Must(BeEnum<FundingApplicantType>).WithMessage("Invalid ApplicantType.");
        RuleFor(x => x.ApplicantLabel).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RequestedAmount).GreaterThan(0);
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Justification).MaximumLength(4000);
        RuleFor(x => x.ContactName).MaximumLength(160);
        RuleFor(x => x.ContactPhone).MaximumLength(40);
        RuleFor(x => x.ContactEmail).MaximumLength(200);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class SubmitFundingReviewRequestValidator : AbstractValidator<SubmitFundingReviewRequest>
{
    public SubmitFundingReviewRequestValidator()
    {
        RuleFor(x => x.Decision).NotEmpty()
            .Must(v => Enum.TryParse<FundingReviewDecision>(v, true, out _))
            .WithMessage("Decision must be one of: Approve, Reject, RequestChanges.");
        RuleFor(x => x.Score).InclusiveBetween(0, 100).When(x => x.Score.HasValue);
        RuleFor(x => x.RecommendedAmount).GreaterThanOrEqualTo(0).When(x => x.RecommendedAmount.HasValue);
        RuleFor(x => x.Comments).MaximumLength(4000);
    }
}

public class DecideFundingApplicationRequestValidator : AbstractValidator<DecideFundingApplicationRequest>
{
    public DecideFundingApplicationRequestValidator()
    {
        RuleFor(x => x.Outcome).NotEmpty()
            .Must(v => v.Equals("Approved", StringComparison.OrdinalIgnoreCase)
                || v.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Outcome must be Approved or Rejected.");
        RuleFor(x => x.ApprovedAmount).GreaterThan(0)
            .When(x => x.Outcome.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            .WithMessage("ApprovedAmount is required and must be greater than zero when approving.");
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}

public class ScheduleFundingDisbursementRequestValidator : AbstractValidator<ScheduleFundingDisbursementRequest>
{
    public ScheduleFundingDisbursementRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).NotEmpty()
            .Must(v => Enum.TryParse<FundingDisbursementMethod>(v, true, out _))
            .WithMessage("Method must be one of: BankTransfer, MobileMoney, Cheque, InKind, Other.");
        RuleFor(x => x.ScheduledFor).NotEmpty();
        RuleFor(x => x.Reference).MaximumLength(120);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class UpdateFundingDisbursementStatusRequestValidator : AbstractValidator<UpdateFundingDisbursementStatusRequest>
{
    public UpdateFundingDisbursementStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(v => Enum.TryParse<FundingDisbursementStatus>(v, true, out _))
            .WithMessage("Status must be one of: Scheduled, Paid, Failed, Cancelled.");
        RuleFor(x => x.Reference).MaximumLength(120);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class RecordLoanRepaymentRequestValidator : AbstractValidator<RecordLoanRepaymentRequest>
{
    public RecordLoanRepaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class WithdrawFundingApplicationRequestValidator : AbstractValidator<WithdrawFundingApplicationRequest>
{
    public WithdrawFundingApplicationRequestValidator()
        => RuleFor(x => x.Reason).MaximumLength(2000);
}

public class AddFundingApplicationNoteRequestValidator : AbstractValidator<AddFundingApplicationNoteRequest>
{
    public AddFundingApplicationNoteRequestValidator()
        => RuleFor(x => x.Note).NotEmpty().MaximumLength(4000);
}
