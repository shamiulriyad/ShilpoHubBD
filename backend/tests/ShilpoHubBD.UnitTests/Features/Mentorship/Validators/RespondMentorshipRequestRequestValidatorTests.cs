using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Mentorship;
using ShilpoHubBD.Application.Validators.Mentorship;

namespace ShilpoHubBD.UnitTests.Features.Mentorship.Validators;

[Trait("Feature", "Mentorship")]
[Trait("Layer", "Validator")]
public class RespondMentorshipRequestRequestValidatorTests
{
    private readonly RespondMentorshipRequestRequestValidator _validator = new();

    [Fact]
    public void Validate_NullResponseMessage_HasNoErrors()
        => _validator.TestValidate(new RespondMentorshipRequestRequest { ResponseMessage = null }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ResponseMessageTooLong_FailsOnResponseMessage()
        => _validator.TestValidate(new RespondMentorshipRequestRequest { ResponseMessage = new string('a', 2001) }).ShouldHaveValidationErrorFor(x => x.ResponseMessage);

    [Fact]
    public void Validate_ResponseMessageAtMaxLength_HasNoError()
        => _validator.TestValidate(new RespondMentorshipRequestRequest { ResponseMessage = new string('a', 2000) }).ShouldNotHaveValidationErrorFor(x => x.ResponseMessage);
}
