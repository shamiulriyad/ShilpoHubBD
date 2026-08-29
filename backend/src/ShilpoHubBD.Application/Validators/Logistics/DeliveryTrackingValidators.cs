using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentRequestValidator()
    {
        RuleFor(x => x.ServiceLevel)
            .Must(v => Enum.TryParse<ShipmentServiceLevel>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ServiceLevel))
            .WithMessage("ServiceLevel must be one of: Economy, Standard, Express, SameDay.");

        RuleFor(x => x.OriginContactName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.OriginPhone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.OriginAddressLine).NotEmpty().MaximumLength(400);
        RuleFor(x => x.OriginCity).NotEmpty().MaximumLength(120);
        RuleFor(x => x.OriginPostalCode).MaximumLength(20);

        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.DestinationAddressLine).NotEmpty().MaximumLength(400);
        RuleFor(x => x.DestinationCity).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DestinationPostalCode).MaximumLength(20);

        RuleFor(x => x.ParcelCount).InclusiveBetween(1, 100000);
        RuleFor(x => x.TotalWeightKg).GreaterThan(0).When(x => x.TotalWeightKg.HasValue);
        RuleFor(x => x.DimensionsNote).MaximumLength(400);
        RuleFor(x => x.DeclaredValue).GreaterThanOrEqualTo(0).When(x => x.DeclaredValue.HasValue);
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0).When(x => x.ShippingCost.HasValue);
        RuleFor(x => x.CodAmount).GreaterThan(0).When(x => x.IsCashOnDelivery);
    }
}

public class UpdateShipmentRequestValidator : AbstractValidator<UpdateShipmentRequest>
{
    public UpdateShipmentRequestValidator()
    {
        RuleFor(x => x.ServiceLevel)
            .Must(v => Enum.TryParse<ShipmentServiceLevel>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ServiceLevel))
            .WithMessage("ServiceLevel must be one of: Economy, Standard, Express, SameDay.");

        RuleFor(x => x.OriginContactName).MaximumLength(160);
        RuleFor(x => x.OriginPhone).MaximumLength(40);
        RuleFor(x => x.OriginAddressLine).MaximumLength(400);
        RuleFor(x => x.OriginCity).MaximumLength(120);
        RuleFor(x => x.OriginPostalCode).MaximumLength(20);
        RuleFor(x => x.RecipientName).MaximumLength(160);
        RuleFor(x => x.RecipientPhone).MaximumLength(40);
        RuleFor(x => x.DestinationAddressLine).MaximumLength(400);
        RuleFor(x => x.DestinationCity).MaximumLength(120);
        RuleFor(x => x.DestinationPostalCode).MaximumLength(20);
        RuleFor(x => x.ParcelCount).InclusiveBetween(1, 100000).When(x => x.ParcelCount.HasValue);
        RuleFor(x => x.TotalWeightKg).GreaterThan(0).When(x => x.TotalWeightKg.HasValue);
        RuleFor(x => x.DimensionsNote).MaximumLength(400);
        RuleFor(x => x.DeclaredValue).GreaterThanOrEqualTo(0).When(x => x.DeclaredValue.HasValue);
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0).When(x => x.ShippingCost.HasValue);
        RuleFor(x => x.CodAmount).GreaterThan(0).When(x => x.CodAmount.HasValue);
    }
}

public class UpdateShipmentStatusRequestValidator : AbstractValidator<UpdateShipmentStatusRequest>
{
    public UpdateShipmentStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(v => Enum.TryParse<ShipmentStatus>(v, true, out _))
            .WithMessage("Status must be a valid shipment status.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.LocationLabel).MaximumLength(200);
        RuleFor(x => x.FailureReason).MaximumLength(1000);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}

public class AddShipmentTrackingEventRequestValidator : AbstractValidator<AddShipmentTrackingEventRequest>
{
    public AddShipmentTrackingEventRequestValidator()
    {
        RuleFor(x => x.EventType).NotEmpty()
            .Must(v => Enum.TryParse<ShipmentEventType>(v, true, out _))
            .WithMessage("EventType must be a valid shipment event type.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.LocationLabel).MaximumLength(200);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}

public class UpdateShipmentLocationRequestValidator : AbstractValidator<UpdateShipmentLocationRequest>
{
    public UpdateShipmentLocationRequestValidator()
    {
        RuleFor(x => x.LocationLabel).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}

public class RecordDeliveryAttemptRequestValidator : AbstractValidator<RecordDeliveryAttemptRequest>
{
    public RecordDeliveryAttemptRequestValidator()
    {
        RuleFor(x => x.Outcome).NotEmpty()
            .Must(v => Enum.TryParse<DeliveryAttemptOutcome>(v, true, out _))
            .WithMessage("Outcome must be one of: Delivered, RecipientUnavailable, AddressNotFound, Refused, Rescheduled, Damaged, Other.");
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.ReceivedByName).MaximumLength(160);
        RuleFor(x => x.ProofOfDeliveryNote).MaximumLength(2000);
        RuleFor(x => x.SignatureImageUrl).MaximumLength(1000);
    }
}

public class MarkShipmentDeliveredRequestValidator : AbstractValidator<MarkShipmentDeliveredRequest>
{
    public MarkShipmentDeliveredRequestValidator()
    {
        RuleFor(x => x.ReceivedByName).MaximumLength(160);
        RuleFor(x => x.ProofOfDeliveryNote).MaximumLength(2000);
        RuleFor(x => x.SignatureImageUrl).MaximumLength(1000);
    }
}

public class CancelShipmentRequestValidator : AbstractValidator<CancelShipmentRequest>
{
    public CancelShipmentRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class AddShipmentNoteRequestValidator : AbstractValidator<AddShipmentNoteRequest>
{
    public AddShipmentNoteRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
