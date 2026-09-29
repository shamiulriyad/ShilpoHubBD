using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.Validators.Auction;

namespace ShilpoHubBD.UnitTests.Features.Auction.Validators;

[Trait("Feature", "Auction")]
[Trait("Layer", "Validator")]
public class PlaceBidRequestValidatorTests
{
    private readonly PlaceBidRequestValidator _validator = new();

    [Fact]
    public void Validate_PositiveAmount_HasNoErrors()
        => _validator.TestValidate(new PlaceBidRequest { Amount = 100 }).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AmountNotPositive_FailsOnAmount(decimal amount)
        => _validator.TestValidate(new PlaceBidRequest { Amount = amount }).ShouldHaveValidationErrorFor(x => x.Amount);
}
