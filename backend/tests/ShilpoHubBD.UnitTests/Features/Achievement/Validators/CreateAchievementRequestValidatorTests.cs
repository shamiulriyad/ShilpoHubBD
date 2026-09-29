using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.Validators.Achievement;

namespace ShilpoHubBD.UnitTests.Features.Achievement.Validators;

[Trait("Feature", "Achievement")]
[Trait("Layer", "Validator")]
public class CreateAchievementRequestValidatorTests
{
    private readonly CreateAchievementRequestValidator _validator = new();

    private static CreateAchievementRequest Valid() => new() { Name = "First Sale", Description = "Sell your first item", RequiredXp = 100, XpReward = 10 };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyName_FailsOnName()
    {
        var request = Valid();
        request.Name = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_FailsOnName()
    {
        var request = Valid();
        request.Name = new string('a', 101);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyDescription_FailsOnDescription()
    {
        var request = Valid();
        request.Description = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsOnDescription()
    {
        var request = Valid();
        request.Description = new string('a', 501);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NegativeRequiredXp_FailsOnRequiredXp()
    {
        var request = Valid();
        request.RequiredXp = -1;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.RequiredXp);
    }

    [Fact]
    public void Validate_NegativeXpReward_FailsOnXpReward()
    {
        var request = Valid();
        request.XpReward = -1;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.XpReward);
    }

    [Fact]
    public void Validate_ZeroRequiredXpAndXpReward_HasNoErrors()
    {
        var request = Valid();
        request.RequiredXp = 0;
        request.XpReward = 0;
        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
