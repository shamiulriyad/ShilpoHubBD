using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDiscovery;

namespace ShilpoHubBD.Application.Validators.HeritageDiscovery;

public class ReorderStopsRequestValidator : AbstractValidator<ReorderStopsRequest>
{
    public ReorderStopsRequestValidator()
    {
        RuleFor(x => x.StopIds).NotEmpty();
        RuleFor(x => x.StopIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("StopIds must not contain duplicates.");
    }
}
