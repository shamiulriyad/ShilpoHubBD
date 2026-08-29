using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type)
            .Must(v => Enum.TryParse<WarehouseType>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Type))
            .WithMessage("Type must be one of: Distribution, Fulfillment, ColdStorage, CrossDock, Returns, Hub.");
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(400);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.ContactPersonName).MaximumLength(160);
        RuleFor(x => x.ContactPhone).MaximumLength(40);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.TotalCapacityUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class UpdateWarehouseRequestValidator : AbstractValidator<UpdateWarehouseRequest>
{
    public UpdateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.Type)
            .Must(v => Enum.TryParse<WarehouseType>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Type))
            .WithMessage("Type must be a valid warehouse type.");
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<WarehouseStatus>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: Active, Inactive, Maintenance, Closed.");
        RuleFor(x => x.AddressLine).MaximumLength(400);
        RuleFor(x => x.City).MaximumLength(120);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.ContactPersonName).MaximumLength(160);
        RuleFor(x => x.ContactPhone).MaximumLength(40);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.TotalCapacityUnits).GreaterThanOrEqualTo(0).When(x => x.TotalCapacityUnits.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class UpsertWarehouseZoneRequestValidator : AbstractValidator<UpsertWarehouseZoneRequest>
{
    public UpsertWarehouseZoneRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Type).NotEmpty()
            .Must(v => Enum.TryParse<WarehouseZoneType>(v, true, out _))
            .WithMessage("Type must be one of: Receiving, Storage, Picking, Packing, Dispatch, Returns, ColdStorage, Quarantine, Staging.");
        RuleFor(x => x.CapacityUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class UpsertWarehouseBinRequestValidator : AbstractValidator<UpsertWarehouseBinRequest>
{
    public UpsertWarehouseBinRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Label).MaximumLength(160);
        RuleFor(x => x.Type).NotEmpty()
            .Must(v => Enum.TryParse<WarehouseBinType>(v, true, out _))
            .WithMessage("Type must be one of: Shelf, Rack, Pallet, Floor, Bulk, Bin, ColdUnit.");
        RuleFor(x => x.CapacityUnits).GreaterThanOrEqualTo(0);
    }
}
