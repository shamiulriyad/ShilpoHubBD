using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Mentorship;
using ShilpoHubBD.Application.Validators.Mentorship;

namespace ShilpoHubBD.UnitTests.Features.Mentorship.Validators;

[Trait("Feature", "Mentorship")]
[Trait("Layer", "Validator")]
public class CreateMentorshipRequestRequestValidatorTests
{
    private readonly CreateMentorshipRequestRequestValidator _validator = new();

    private static CreateMentorshipRequestRequest Valid() => new() { MentorProfileId = Guid.NewGuid(), Message = "Please teach me" };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyMentorProfileId_FailsOnMentorProfileId()
    {
        var request = Valid();
        request.MentorProfileId = Guid.Empty;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.MentorProfileId);
    }

    [Fact]
    public void Validate_EmptyMessage_FailsOnMessage()
    {
        var request = Valid();
        request.Message = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Validate_MessageTooLong_FailsOnMessage()
    {
        var request = Valid();
        request.Message = new string('a', 2001);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Validate_NullHeritageSkillId_HasNoError()
    {
        var request = Valid();
        request.HeritageSkillId = null;
        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
