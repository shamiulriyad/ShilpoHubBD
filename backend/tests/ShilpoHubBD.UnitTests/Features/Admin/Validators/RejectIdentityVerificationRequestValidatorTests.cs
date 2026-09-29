using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Validators.Admin;

namespace ShilpoHubBD.UnitTests.Features.Admin.Validators;

[Trait("Feature", "Admin")]
[Trait("Layer", "Validator")]
public class RejectIdentityVerificationRequestValidatorTests
{
    private readonly RejectIdentityVerificationRequestValidator _validator = new();

    [Fact]
    public void Validate_ReasonGiven_HasNoErrors()
        => _validator.TestValidate(new RejectIdentityVerificationRequest { RejectionReason = "Blurry document photo." })
            .ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ReasonMissing_FailsOnRejectionReason(string reason)
        => _validator.TestValidate(new RejectIdentityVerificationRequest { RejectionReason = reason })
            .ShouldHaveValidationErrorFor(x => x.RejectionReason);

    [Fact]
    public void Validate_ReasonAtMaximumLength_Passes()
        => _validator.TestValidate(new RejectIdentityVerificationRequest { RejectionReason = new string('r', 1000) })
            .ShouldNotHaveValidationErrorFor(x => x.RejectionReason);

    [Fact]
    public void Validate_ReasonOverMaximumLength_FailsOnRejectionReason()
        => _validator.TestValidate(new RejectIdentityVerificationRequest { RejectionReason = new string('r', 1001) })
            .ShouldHaveValidationErrorFor(x => x.RejectionReason);
}
