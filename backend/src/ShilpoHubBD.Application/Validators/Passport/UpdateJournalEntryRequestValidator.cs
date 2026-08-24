using FluentValidation;
using ShilpoHubBD.Application.DTOs.Passport;

namespace ShilpoHubBD.Application.Validators.Passport;

public class UpdateJournalEntryRequestValidator : AbstractValidator<UpdateJournalEntryRequest>
{
    public UpdateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.PhotoUrl).MaximumLength(500);
    }
}
