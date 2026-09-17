using FluentValidation;
using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Validators.Security;

public class BlockIpRequestValidator : AbstractValidator<BlockIpRequest>
{
    public BlockIpRequestValidator()
    {
        RuleFor(x => x.IpAddress).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
