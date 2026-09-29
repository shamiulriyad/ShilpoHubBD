using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Validators.Reviews;

namespace ShilpoHubBD.UnitTests.Features.Reviews.Validators;

[Trait("Feature", "Reviews")]
[Trait("Layer", "Validator")]
public class CreateReviewRequestValidatorTests
{
    private readonly CreateReviewRequestValidator _validator = new();

    private static CreateReviewRequest Valid() => new() { ProductId = Guid.NewGuid(), Rating = 5, Comment = "Beautiful craftsmanship." };

    [Fact]
    public void Validate_ValidProductReview_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ValidHeritagePlaceReview_HasNoErrors()
        => _validator.TestValidate(new CreateReviewRequest { HeritagePlaceId = Guid.NewGuid(), Rating = 4, Comment = "x" })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_ValidBookingReview_HasNoErrors()
        => _validator.TestValidate(new CreateReviewRequest { BookingId = Guid.NewGuid(), Rating = 3, Comment = "x" })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_NoSubjectSet_FailsWithTheExactlyOneMessage()
    {
        var result = _validator.TestValidate(new CreateReviewRequest { Rating = 5, Comment = "x" });

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Exactly one of ProductId, HeritagePlaceId or BookingId must be set.");
    }

    [Fact]
    public void Validate_TwoSubjectsSet_FailsWithTheExactlyOneMessage()
    {
        var result = _validator.TestValidate(new CreateReviewRequest { ProductId = Guid.NewGuid(), HeritagePlaceId = Guid.NewGuid(), Rating = 5, Comment = "x" });

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Exactly one of ProductId, HeritagePlaceId or BookingId must be set.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Validate_RatingAtTheBounds_Passes(int rating)
    {
        var request = Valid();
        request.Rating = rating;

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_RatingOutsideOneToFive_FailsOnRating(int rating)
    {
        var request = Valid();
        request.Rating = rating;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Validate_NoProducerRating_HasNoErrors()
    {
        var request = Valid();
        request.ProducerRating = null;

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.ProducerRating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_ProducerRatingGivenButOutsideOneToFive_FailsOnProducerRating(int rating)
    {
        var request = Valid();
        request.ProducerRating = rating;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ProducerRating);
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
        request.ImageUrls = new List<string> { "https://example.com/a.jpg", "" };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("ImageUrls[1]");
    }

    [Fact]
    public void Validate_ImageUrlsAllValid_HasNoErrors()
    {
        var request = Valid();
        request.ImageUrls = new List<string> { "https://example.com/a.jpg", "https://example.com/b.jpg" };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
