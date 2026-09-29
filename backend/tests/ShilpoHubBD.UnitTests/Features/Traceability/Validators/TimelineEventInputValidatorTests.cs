using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Validators.Traceability;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Validators;

[Trait("Feature", "Traceability")]
[Trait("Layer", "Validator")]
public class TimelineEventInputValidatorTests
{
    private readonly TimelineEventInputValidator _validator = new();

    private static TimelineEventInput Valid() => new() { Title = "Woven", Description = "Woven by hand", EventDate = DateTime.UtcNow };

    [Fact]
    public void Validate_ValidInput_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyTitle_FailsOnTitle()
    {
        var input = Valid();
        input.Title = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_TitleTooLong_FailsOnTitle()
    {
        var input = Valid();
        input.Title = new string('a', 201);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_EmptyDescription_FailsOnDescription()
    {
        var input = Valid();
        input.Description = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsOnDescription()
    {
        var input = Valid();
        input.Description = new string('a', 1001);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullLocation_HasNoError()
    {
        var input = Valid();
        input.Location = null;
        _validator.TestValidate(input).ShouldNotHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Validate_LocationTooLong_FailsOnLocation()
    {
        var input = Valid();
        input.Location = new string('a', 201);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Validate_DefaultEventDate_FailsOnEventDate()
    {
        var input = Valid();
        input.EventDate = default;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.EventDate);
    }
}
