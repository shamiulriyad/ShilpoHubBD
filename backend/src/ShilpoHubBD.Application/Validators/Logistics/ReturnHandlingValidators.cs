using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class ReturnItemInputValidator : AbstractValidator<ReturnItemInput>
{
    public ReturnItemInputValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Sku).MaximumLength(80);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitRefundAmount).GreaterThanOrEqualTo(0).When(x => x.UnitRefundAmount.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class CreateReturnRequestRequestValidator : AbstractValidator<CreateReturnRequestRequest>
{
    public CreateReturnRequestRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty()
            .Must(v => Enum.TryParse<ReturnReason>(v, true, out _))
            .WithMessage("Reason must be a valid return reason.");
        RuleFor(x => x.ReasonDetail).MaximumLength(2000);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.PickupContactName).MaximumLength(160);
        RuleFor(x => x.PickupPhone).MaximumLength(40);
        RuleFor(x => x.PickupAddressLine).MaximumLength(400);
        RuleFor(x => x.PickupCity).MaximumLength(120);
        RuleFor(x => x.PickupPostalCode).MaximumLength(20);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A return must have at least one item.");
        RuleForEach(x => x.Items).SetValidator(new ReturnItemInputValidator());
    }
}

public class UpdateReturnRequestRequestValidator : AbstractValidator<UpdateReturnRequestRequest>
{
    public UpdateReturnRequestRequestValidator()
    {
        RuleFor(x => x.Reason)
            .Must(v => Enum.TryParse<ReturnReason>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Reason must be a valid return reason.");
        RuleFor(x => x.ReasonDetail).MaximumLength(2000);
        RuleFor(x => x.CustomerName).MaximumLength(160);
        RuleFor(x => x.CustomerPhone).MaximumLength(40);
        RuleFor(x => x.PickupContactName).MaximumLength(160);
        RuleFor(x => x.PickupPhone).MaximumLength(40);
        RuleFor(x => x.PickupAddressLine).MaximumLength(400);
        RuleFor(x => x.PickupCity).MaximumLength(120);
        RuleFor(x => x.PickupPostalCode).MaximumLength(20);
        RuleForEach(x => x.Items).SetValidator(new ReturnItemInputValidator()).When(x => x.Items is not null);
    }
}

public class ApproveReturnRequestRequestValidator : AbstractValidator<ApproveReturnRequestRequest>
{
    public ApproveReturnRequestRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class RejectReturnRequestRequestValidator : AbstractValidator<RejectReturnRequestRequest>
{
    public RejectReturnRequestRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class ScheduleReturnPickupRequestValidator : AbstractValidator<ScheduleReturnPickupRequest>
{
    public ScheduleReturnPickupRequestValidator()
    {
        RuleFor(x => x.ScheduledPickupAt).NotEmpty();
        RuleFor(x => x.PickupContactName).MaximumLength(160);
        RuleFor(x => x.PickupPhone).MaximumLength(40);
        RuleFor(x => x.PickupAddressLine).MaximumLength(400);
        RuleFor(x => x.PickupCity).MaximumLength(120);
        RuleFor(x => x.PickupPostalCode).MaximumLength(20);
        RuleFor(x => x.AssignedCarrierLabel).MaximumLength(80);
        RuleFor(x => x.AssignedDriverName).MaximumLength(160);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class UpdateReturnStatusRequestValidator : AbstractValidator<UpdateReturnStatusRequest>
{
    public UpdateReturnStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(v => Enum.TryParse<ReturnStatus>(v, true, out _))
            .WithMessage("Status must be one of: InTransit, Received, UnderInspection, Closed.");
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class ReturnItemAssessmentInputValidator : AbstractValidator<ReturnItemAssessmentInput>
{
    public ReturnItemAssessmentInputValidator()
    {
        RuleFor(x => x.ReturnItemId).NotEmpty();
        RuleFor(x => x.QuantityReceived).GreaterThanOrEqualTo(0).When(x => x.QuantityReceived.HasValue);
        RuleFor(x => x.Condition)
            .Must(v => Enum.TryParse<ReturnItemCondition>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Condition))
            .WithMessage("Condition must be a valid return item condition.");
        RuleFor(x => x.Disposition)
            .Must(v => Enum.TryParse<ReturnDisposition>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Disposition))
            .WithMessage("Disposition must be a valid return disposition.");
        RuleFor(x => x.UnitRefundAmount).GreaterThanOrEqualTo(0).When(x => x.UnitRefundAmount.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class RecordReturnInspectionRequestValidator : AbstractValidator<RecordReturnInspectionRequest>
{
    public RecordReturnInspectionRequestValidator()
    {
        RuleFor(x => x.OverallCondition).NotEmpty()
            .Must(v => Enum.TryParse<ReturnItemCondition>(v, true, out _))
            .WithMessage("OverallCondition must be a valid return item condition.");
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RecommendedResolution).NotEmpty()
            .Must(v => Enum.TryParse<ReturnResolutionType>(v, true, out _))
            .WithMessage("RecommendedResolution must be one of: Refund, Replacement, Repair, StoreCredit, NoAction.");
        RuleFor(x => x.PhotosJson).MaximumLength(8000);
        RuleForEach(x => x.ItemAssessments).SetValidator(new ReturnItemAssessmentInputValidator());
    }
}

public class RestockReturnItemInputValidator : AbstractValidator<RestockReturnItemInput>
{
    public RestockReturnItemInputValidator()
    {
        RuleFor(x => x.ReturnItemId).NotEmpty();
        RuleFor(x => x.RestockedQuantity).GreaterThanOrEqualTo(0);
    }
}

public class RestockReturnRequestValidator : AbstractValidator<RestockReturnRequest>
{
    public RestockReturnRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
        RuleForEach(x => x.Items).SetValidator(new RestockReturnItemInputValidator());
    }
}

public class RecordReturnRefundRequestValidator : AbstractValidator<RecordReturnRefundRequest>
{
    public RecordReturnRefundRequestValidator()
    {
        RuleFor(x => x.RefundAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ResolutionType)
            .Must(v => Enum.TryParse<ReturnResolutionType>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ResolutionType))
            .WithMessage("ResolutionType must be one of: Refund, Replacement, Repair, StoreCredit, NoAction.");
        RuleFor(x => x.ResolutionNote).MaximumLength(2000);
        RuleFor(x => x.RefundMethod).MaximumLength(40);
        RuleFor(x => x.RefundReference).MaximumLength(120);
    }
}

public class CancelReturnRequestRequestValidator : AbstractValidator<CancelReturnRequestRequest>
{
    public CancelReturnRequestRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class AddReturnNoteRequestValidator : AbstractValidator<AddReturnNoteRequest>
{
    public AddReturnNoteRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
