using FluentValidation;
using ShilpoHubBD.Application.DTOs.BusinessPartner;

namespace ShilpoHubBD.Application.Validators.BusinessPartner;

public class UpsertBusinessPartnerProfileRequestValidator : AbstractValidator<UpsertBusinessPartnerProfileRequest>
{
    public UpsertBusinessPartnerProfileRequestValidator()
    {
        RuleFor(x => x.BusinessType).IsInEnum();
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TaxIdentificationNumber).MaximumLength(100);
        RuleFor(x => x.YearEstablished).InclusiveBetween(1700, 2100).When(x => x.YearEstablished.HasValue);
        RuleFor(x => x.Industry).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BusinessSize).IsInEnum();
        RuleFor(x => x.EmployeeCount).GreaterThanOrEqualTo(1).When(x => x.EmployeeCount.HasValue);
        RuleFor(x => x.Website).MaximumLength(300);
        RuleFor(x => x.CompanyDescription).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);

        RuleFor(x => x.ContactPersonName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactPersonDesignation).MaximumLength(150);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(200);

        RuleFor(x => x.MinimumOrderQuantity).GreaterThanOrEqualTo(1).When(x => x.MinimumOrderQuantity.HasValue);
        RuleFor(x => x.MaxBudgetPerOrder).GreaterThanOrEqualTo(0).When(x => x.MaxBudgetPerOrder.HasValue);
        RuleFor(x => x.PreferredOrderFrequency).IsInEnum().When(x => x.PreferredOrderFrequency.HasValue);
        RuleFor(x => x.PreferredPaymentTerms).MaximumLength(200);
        RuleFor(x => x.PreferredCategoryIds).Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("PreferredCategoryIds must not contain duplicates.");

        RuleForEach(x => x.Documents).SetValidator(new BusinessDocumentInputValidator());
    }
}
