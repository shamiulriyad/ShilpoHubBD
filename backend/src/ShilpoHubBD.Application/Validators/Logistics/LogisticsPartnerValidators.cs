using FluentValidation;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Validators.Logistics;

public class UpsertLogisticsPartnerProfileRequestValidator : AbstractValidator<UpsertLogisticsPartnerProfileRequest>
{
    public UpsertLogisticsPartnerProfileRequestValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LegalName).MaximumLength(200);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.ContactPersonName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.BaseAddressLine).NotEmpty().MaximumLength(400);
        RuleFor(x => x.BaseCity).NotEmpty().MaximumLength(120);
        RuleFor(x => x.BasePostalCode).MaximumLength(20);
        RuleFor(x => x.Country).MaximumLength(80);
        RuleFor(x => x.FleetSize).InclusiveBetween(0, 100000);
        RuleFor(x => x.MaxDailyPickups).InclusiveBetween(0, 1000000);
        RuleFor(x => x.MaxVehicleCapacityKg).GreaterThan(0).When(x => x.MaxVehicleCapacityKg.HasValue);
        RuleFor(x => x.OperatingDayStartHour).InclusiveBetween(0, 24).When(x => x.OperatingDayStartHour.HasValue);
        RuleFor(x => x.OperatingDayEndHour).InclusiveBetween(0, 24).When(x => x.OperatingDayEndHour.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class VerifyLogisticsPartnerRequestValidator : AbstractValidator<VerifyLogisticsPartnerRequest>
{
    public VerifyLogisticsPartnerRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(v => Enum.TryParse<LogisticsPartnerVerificationStatus>(v, true, out _))
            .WithMessage("Status must be one of: Pending, Verified, Rejected, Suspended.");
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class UpsertLogisticsServiceAreaRequestValidator : AbstractValidator<UpsertLogisticsServiceAreaRequest>
{
    public UpsertLogisticsServiceAreaRequestValidator()
    {
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.StandardDeliveryDays).InclusiveBetween(0, 60);
        RuleFor(x => x.SurchargeAmount).GreaterThanOrEqualTo(0).When(x => x.SurchargeAmount.HasValue);
    }
}
