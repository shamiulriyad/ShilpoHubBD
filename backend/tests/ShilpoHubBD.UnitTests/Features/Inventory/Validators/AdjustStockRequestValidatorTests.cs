using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Inventory;
using ShilpoHubBD.Application.Validators.Inventory;

namespace ShilpoHubBD.UnitTests.Features.Inventory.Validators;

[Trait("Feature", "Inventory")]
[Trait("Layer", "Validator")]
public class AdjustStockRequestValidatorTests
{
    private readonly AdjustStockRequestValidator _validator = new();

    [Theory]
    [InlineData(5)]
    [InlineData(-5)]
    public void Validate_NonZeroChangeAmountWithAReason_HasNoErrors(int amount)
        => _validator.TestValidate(new AdjustStockRequest { ChangeAmount = amount, Reason = "Restock" }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ZeroChangeAmount_FailsWithTheMustNotBeZeroMessage()
        => _validator.TestValidate(new AdjustStockRequest { ChangeAmount = 0, Reason = "x" })
            .ShouldHaveValidationErrorFor(x => x.ChangeAmount)
            .WithErrorMessage("ChangeAmount must not be zero.");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ReasonMissing_FailsOnReason(string reason)
        => _validator.TestValidate(new AdjustStockRequest { ChangeAmount = 1, Reason = reason }).ShouldHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void Validate_ReasonAtMaximumLength_Passes()
        => _validator.TestValidate(new AdjustStockRequest { ChangeAmount = 1, Reason = new string('r', 500) })
            .ShouldNotHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void Validate_ReasonOverMaximumLength_FailsOnReason()
        => _validator.TestValidate(new AdjustStockRequest { ChangeAmount = 1, Reason = new string('r', 501) })
            .ShouldHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void Validate_VariantIdIsOptional()
        => _validator.TestValidate(new AdjustStockRequest { ChangeAmount = 1, Reason = "x", VariantId = Guid.NewGuid() })
            .ShouldNotHaveAnyValidationErrors();
}
