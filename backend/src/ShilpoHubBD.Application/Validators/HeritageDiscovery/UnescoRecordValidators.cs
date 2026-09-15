using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class CreateUnescoRecordRequestValidator : AbstractValidator<CreateUnescoRecordRequest>
{
    public CreateUnescoRecordRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().Must(BeEnum<UnescoRecordType>).WithMessage("Invalid Type.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.InscribedYear).InclusiveBetween(1900, 2100);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.OfficialUrl).MaximumLength(1000);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class UpdateUnescoRecordRequestValidator : AbstractValidator<UpdateUnescoRecordRequest>
{
    public UpdateUnescoRecordRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().Must(BeEnum<UnescoRecordType>).WithMessage("Invalid Type.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.InscribedYear).InclusiveBetween(1900, 2100);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.OfficialUrl).MaximumLength(1000);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}
