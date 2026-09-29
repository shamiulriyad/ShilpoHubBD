using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Application.Validators.MentorMatching;

namespace ShilpoHubBD.UnitTests.Features.MentorMatching.Validators;

[Trait("Feature", "MentorMatching")]
[Trait("Layer", "Validator")]
public class MentorMatchRequestValidatorTests
{
    private readonly MentorMatchRequestValidator _validator = new();

    [Fact]
    public void Validate_Defaults_HaveNoErrors()
        => _validator.TestValidate(new MentorMatchRequest()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_NegativeMinYearsOfExperience_FailsOnMinYearsOfExperience()
        => _validator.TestValidate(new MentorMatchRequest { MinYearsOfExperience = -1 }).ShouldHaveValidationErrorFor(x => x.MinYearsOfExperience);

    [Fact]
    public void Validate_NullMinYearsOfExperience_HasNoError()
        => _validator.TestValidate(new MentorMatchRequest { MinYearsOfExperience = null }).ShouldNotHaveValidationErrorFor(x => x.MinYearsOfExperience);

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_MaxResultsOutsideOneToFifty_FailsOnMaxResults(int maxResults)
        => _validator.TestValidate(new MentorMatchRequest { MaxResults = maxResults }).ShouldHaveValidationErrorFor(x => x.MaxResults);

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Validate_MaxResultsAtTheBounds_Passes(int maxResults)
        => _validator.TestValidate(new MentorMatchRequest { MaxResults = maxResults }).ShouldNotHaveValidationErrorFor(x => x.MaxResults);
}
