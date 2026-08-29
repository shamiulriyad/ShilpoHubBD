using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class RouteStopInputValidator : AbstractValidator<RouteStopInput>
{
    public RouteStopInputValidator()
    {
        RuleFor(x => x.StopType).NotEmpty()
            .Must(v => Enum.TryParse<DeliveryRouteStopType>(v, true, out _))
            .WithMessage("StopType must be one of: Pickup, Delivery, Transfer, Waypoint.");
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(400);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ContactName).MaximumLength(160);
        RuleFor(x => x.ContactPhone).MaximumLength(40);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.LoadKg).GreaterThanOrEqualTo(0).When(x => x.LoadKg.HasValue);
        RuleFor(x => x.PackageCount).InclusiveBetween(0, 100000);
        RuleFor(x => x.ServiceDurationMinutes).InclusiveBetween(0, 1440).When(x => x.ServiceDurationMinutes.HasValue);
        RuleFor(x => x.Instructions).MaximumLength(2000);
    }
}

public class CreateDeliveryRouteRequestValidator : AbstractValidator<CreateDeliveryRouteRequest>
{
    public CreateDeliveryRouteRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartLocationLabel).MaximumLength(200);
        RuleFor(x => x.EndLocationLabel).MaximumLength(200);
        RuleFor(x => x.StartLatitude).InclusiveBetween(-90, 90).When(x => x.StartLatitude.HasValue);
        RuleFor(x => x.StartLongitude).InclusiveBetween(-180, 180).When(x => x.StartLongitude.HasValue);
        RuleFor(x => x.EndLatitude).InclusiveBetween(-90, 90).When(x => x.EndLatitude.HasValue);
        RuleFor(x => x.EndLongitude).InclusiveBetween(-180, 180).When(x => x.EndLongitude.HasValue);
        RuleFor(x => x.VehicleCapacityKg).GreaterThan(0).When(x => x.VehicleCapacityKg.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleForEach(x => x.Stops).SetValidator(new RouteStopInputValidator());
    }
}

public class UpdateDeliveryRouteRequestValidator : AbstractValidator<UpdateDeliveryRouteRequest>
{
    public UpdateDeliveryRouteRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.StartLocationLabel).MaximumLength(200);
        RuleFor(x => x.EndLocationLabel).MaximumLength(200);
        RuleFor(x => x.StartLatitude).InclusiveBetween(-90, 90).When(x => x.StartLatitude.HasValue);
        RuleFor(x => x.StartLongitude).InclusiveBetween(-180, 180).When(x => x.StartLongitude.HasValue);
        RuleFor(x => x.EndLatitude).InclusiveBetween(-90, 90).When(x => x.EndLatitude.HasValue);
        RuleFor(x => x.EndLongitude).InclusiveBetween(-180, 180).When(x => x.EndLongitude.HasValue);
        RuleFor(x => x.VehicleCapacityKg).GreaterThan(0).When(x => x.VehicleCapacityKg.HasValue);
        RuleFor(x => x.EstimatedDurationMinutes).InclusiveBetween(0, 100000).When(x => x.EstimatedDurationMinutes.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class UpdateRouteStopRequestValidator : AbstractValidator<UpdateRouteStopRequest>
{
    public UpdateRouteStopRequestValidator()
    {
        RuleFor(x => x.StopType)
            .Must(v => Enum.TryParse<DeliveryRouteStopType>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.StopType))
            .WithMessage("StopType must be one of: Pickup, Delivery, Transfer, Waypoint.");
        RuleFor(x => x.AddressLine).MaximumLength(400);
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.ContactName).MaximumLength(160);
        RuleFor(x => x.ContactPhone).MaximumLength(40);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.LoadKg).GreaterThanOrEqualTo(0).When(x => x.LoadKg.HasValue);
        RuleFor(x => x.PackageCount).InclusiveBetween(0, 100000).When(x => x.PackageCount.HasValue);
        RuleFor(x => x.ServiceDurationMinutes).InclusiveBetween(0, 1440).When(x => x.ServiceDurationMinutes.HasValue);
        RuleFor(x => x.Instructions).MaximumLength(2000);
    }
}

public class ResequenceRouteRequestValidator : AbstractValidator<ResequenceRouteRequest>
{
    public ResequenceRouteRequestValidator()
    {
        RuleFor(x => x.StopIdsInOrder).NotEmpty();
    }
}

public class OptimizeRouteRequestValidator : AbstractValidator<OptimizeRouteRequest>
{
    public OptimizeRouteRequestValidator()
    {
        RuleFor(x => x.Strategy).MaximumLength(40);
        RuleFor(x => x.AverageSpeedKmh).InclusiveBetween(1, 200).When(x => x.AverageSpeedKmh.HasValue);
    }
}

public class AssignRouteRequestValidator : AbstractValidator<AssignRouteRequest>
{
    public AssignRouteRequestValidator()
    {
        RuleFor(x => x.AssignedDriverName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.AssignedDriverPhone).MaximumLength(40);
        RuleFor(x => x.AssignedVehicleLabel).MaximumLength(80);
        RuleFor(x => x.VehicleCapacityKg).GreaterThan(0).When(x => x.VehicleCapacityKg.HasValue);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class RouteTransitionRequestValidator : AbstractValidator<RouteTransitionRequest>
{
    public RouteTransitionRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}

public class CancelRouteRequestValidator : AbstractValidator<CancelRouteRequest>
{
    public CancelRouteRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class CompleteRouteStopRequestValidator : AbstractValidator<CompleteRouteStopRequest>
{
    public CompleteRouteStopRequestValidator()
    {
        RuleFor(x => x.CompletionNote).MaximumLength(2000);
    }
}

public class FailRouteStopRequestValidator : AbstractValidator<FailRouteStopRequest>
{
    public FailRouteStopRequestValidator()
    {
        RuleFor(x => x.FailureReason).NotEmpty().MaximumLength(1000);
    }
}

public class AddRouteNoteRequestValidator : AbstractValidator<AddRouteNoteRequest>
{
    public AddRouteNoteRequestValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
