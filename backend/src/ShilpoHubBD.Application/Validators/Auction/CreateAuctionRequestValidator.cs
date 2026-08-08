using FluentValidation;
using ShilpoHubBD.Application.DTOs.Auction;

namespace ShilpoHubBD.Application.Validators.Auction;

public class CreateAuctionRequestValidator : AbstractValidator<CreateAuctionRequest>
{
    public CreateAuctionRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.StartingPrice).GreaterThan(0);
        RuleFor(x => x.MinBidIncrement).GreaterThan(0);
        RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage("End time must be after start time.");
        RuleFor(x => x.EndAt).GreaterThan(DateTime.UtcNow).WithMessage("End time must be in the future.");
    }
}
