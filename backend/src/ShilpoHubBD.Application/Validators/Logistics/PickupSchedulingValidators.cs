using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class PickupItemRequestValidator : AbstractValidator<PickupItemRequest>
{
    public PickupItemRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.LengthCm).GreaterThan(0).When(x => x.LengthCm.HasValue);
        RuleFor(x => x.WidthCm).GreaterThan(0).When(x => x.WidthCm.HasValue);
        RuleFor(x => x.HeightCm).GreaterThan(0).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.Reference).MaximumLength(120);
    }
}

public class CreatePickupRequestRequestValidator : AbstractValidator<CreatePickupRequestRequest>
{
    public CreatePickupRequestRequestValidator()
    {
        RuleFor(x => x.Priority)
            .Must(v => Enum.TryParse<PickupPriority>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Priority))
            .WithMessage("Priority must be one of: Standard, Express, SameDay.");

        RuleFor(x => x.OriginContactName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.OriginPhone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.OriginAddressLine).NotEmpty().MaximumLength(400);
        RuleFor(x => x.OriginCity).NotEmpty().MaximumLength(120);
        RuleFor(x => x.OriginPostalCode).MaximumLength(20);
        RuleFor(x => x.DestinationContactName).MaximumLength(160);
        RuleFor(x => x.DestinationPhone).MaximumLength(40);
        RuleFor(x => x.DestinationAddressLine).MaximumLength(400);
        RuleFor(x => x.DestinationCity).MaximumLength(120);
        RuleFor(x => x.PackageCount).InclusiveBetween(1, 100000);
        RuleFor(x => x.TotalWeightKg).GreaterThan(0).When(x => x.TotalWeightKg.HasValue);
        RuleFor(x => x.DeclaredValue).GreaterThanOrEqualTo(0).When(x => x.DeclaredValue.HasValue);
        RuleFor(x => x.CodAmount).GreaterThan(0).When(x => x.IsCashOnDelivery);
        RuleFor(x => x.SpecialInstructions).MaximumLength(2000);
        RuleForEach(x => x.Items).SetValidator(new PickupItemRequestValidator());
    }
}

public class UpdatePickupRequestRequestValidator : AbstractValidator<UpdatePickupRequestRequest>
{
    public UpdatePickupRequestRequestValidator()
    {
        RuleFor(x => x.Priority)
            .Must(v => Enum.TryParse<PickupPriority>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Priority))
            .WithMessage("Priority must be one of: Standard, Express, SameDay.");

        RuleFor(x => x.OriginContactName).MaximumLength(160);
        RuleFor(x => x.OriginPhone).MaximumLength(40);
        RuleFor(x => x.OriginAddressLine).MaximumLength(400);
        RuleFor(x => x.OriginCity).MaximumLength(120);
        RuleFor(x => x.OriginPostalCode).MaximumLength(20);
        RuleFor(x => x.DestinationContactName).MaximumLength(160);
        RuleFor(x => x.DestinationPhone).MaximumLength(40);
        RuleFor(x => x.DestinationAddressLine).MaximumLength(400);
        RuleFor(x => x.DestinationCity).MaximumLength(120);
        RuleFor(x => x.PackageCount).InclusiveBetween(1, 100000).When(x => x.PackageCount.HasValue);
        RuleFor(x => x.TotalWeightKg).GreaterThan(0).When(x => x.TotalWeightKg.HasValue);
        RuleFor(x => x.DeclaredValue).GreaterThanOrEqualTo(0).When(x => x.DeclaredValue.HasValue);
        RuleFor(x => x.CodAmount).GreaterThan(0).When(x => x.CodAmount.HasValue);
        RuleFor(x => x.SpecialInstructions).MaximumLength(2000);
        RuleForEach(x => x.Items).SetValidator(new PickupItemRequestValidator()).When(x => x.Items is not null);
    }
}

public class SchedulePickupRequestRequestValidator : AbstractValidator<SchedulePickupRequestRequest>
{
    public SchedulePickupRequestRequestValidator()
    {
        RuleFor(x => x.ScheduledPickupAt).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class AssignPickupRequestRequestValidator : AbstractValidator<AssignPickupRequestRequest>
{
    public AssignPickupRequestRequestValidator()
    {
        RuleFor(x => x.AssignedDriverName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.AssignedDriverPhone).MaximumLength(40);
        RuleFor(x => x.AssignedVehicleLabel).MaximumLength(80);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class UpdatePickupStatusRequestValidator : AbstractValidator<UpdatePickupStatusRequest>
{
    public UpdatePickupStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(v => Enum.TryParse<PickupRequestStatus>(v, true, out _))
            .WithMessage("Status must be one of: EnRoute, Collected, Completed, Failed.");
        RuleFor(x => x.Note).MaximumLength(1000);
        RuleFor(x => x.FailureReason).MaximumLength(1000);
    }
}

public class CancelPickupRequestRequestValidator : AbstractValidator<CancelPickupRequestRequest>
{
    public CancelPickupRequestRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class AddPickupNoteRequestValidator : AbstractValidator<AddPickupNoteRequest>
{
    public AddPickupNoteRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
