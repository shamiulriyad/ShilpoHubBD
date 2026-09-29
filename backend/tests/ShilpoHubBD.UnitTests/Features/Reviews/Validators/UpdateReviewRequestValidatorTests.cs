using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Validators.Reviews;

namespace ShilpoHubBD.UnitTests.Features.Reviews.Validators;

[Trait("Feature", "Reviews")]
[Trait("Layer", "Validator")]
public class UpdateReviewRequestValidatorTests
{
    private readonly UpdateReviewRequestValidator _validator = new();

    private static UpdateReviewRequest Valid() => new() { Rating = 4, Comment = "Updated thoughts." };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_RatingOutsideOneToFive_FailsOnRating(int rating)
    {
        var request = Valid();
        request.Rating = rating;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_CommentMissing_FailsOnComment(string comment)
    {
        var request = Valid();
        request.Comment = comment;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Validate_CommentOverMaximumLength_FailsOnComment()
    {
        var request = Valid();
        request.Comment = new string('a', 2001);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Validate_ImageUrlsEmpty_FailsOnThatEntry()
    {
        var request = Valid();
        request.ImageUrls = new List<string> { "" };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("ImageUrls[0]");
    }
}
