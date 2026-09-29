using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Validators.Complaints;

namespace ShilpoHubBD.UnitTests.Features.Complaints.Validators;

[Trait("Feature", "Complaints")]
[Trait("Layer", "Validator")]
public class RespondToOrderComplaintRequestValidatorTests
{
    private readonly RespondToOrderComplaintRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidMessage_HasNoErrors()
        => _validator.TestValidate(new RespondToOrderComplaintRequest { Message = "Sent a replacement." }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyMessage_FailsOnMessage()
        => _validator.TestValidate(new RespondToOrderComplaintRequest { Message = "" }).ShouldHaveValidationErrorFor(x => x.Message);

    [Fact]
    public void Validate_MessageTooLong_FailsOnMessage()
        => _validator.TestValidate(new RespondToOrderComplaintRequest { Message = new string('a', 2001) }).ShouldHaveValidationErrorFor(x => x.Message);

    [Fact]
    public void Validate_MessageAtMaxLength_HasNoError()
        => _validator.TestValidate(new RespondToOrderComplaintRequest { Message = new string('a', 2000) }).ShouldNotHaveValidationErrorFor(x => x.Message);
}
