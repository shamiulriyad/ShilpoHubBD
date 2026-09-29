using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.Validators.Achievement;

namespace ShilpoHubBD.UnitTests.Features.Achievement.Validators;

[Trait("Feature", "Achievement")]
[Trait("Layer", "Validator")]
public class AwardXpRequestValidatorTests
{
    private readonly AwardXpRequestValidator _validator = new();

    private static AwardXpRequest Valid() => new() { UserId = Guid.NewGuid(), Amount = 10, Reason = "Bonus" };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyUserId_FailsOnUserId()
    {
        var request = Valid();
        request.UserId = Guid.Empty;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AmountNotGreaterThanZero_FailsOnAmount(int amount)
    {
        var request = Valid();
        request.Amount = amount;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_EmptyReason_FailsOnReason()
    {
        var request = Valid();
        request.Reason = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ReasonTooLong_FailsOnReason()
    {
        var request = Valid();
        request.Reason = new string('a', 201);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
