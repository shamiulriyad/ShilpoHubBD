using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.Validators.Auction;

namespace ShilpoHubBD.UnitTests.Features.Auction.Validators;

[Trait("Feature", "Auction")]
[Trait("Layer", "Validator")]
public class CreateAuctionRequestValidatorTests
{
    private readonly CreateAuctionRequestValidator _validator = new();

    private static CreateAuctionRequest Valid() => new()
    {
        ProductId = Guid.NewGuid(), Title = "Antique Jamdani Saree", Description = "A rare heritage piece.",
        StartingPrice = 5000, MinBidIncrement = 100, StartAt = DateTime.UtcNow.AddHours(1), EndAt = DateTime.UtcNow.AddDays(7),
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyProductId_FailsOnProductId()
    {
        var request = Valid();
        request.ProductId = Guid.Empty;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_TitleMissing_FailsOnTitle(string title)
    {
        var request = Valid();
        request.Title = title;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_TitleOverMaximumLength_FailsOnTitle()
    {
        var request = Valid();
        request.Title = new string('a', 201);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_DescriptionMissing_FailsOnDescription(string description)
    {
        var request = Valid();
        request.Description = description;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionOverMaximumLength_FailsOnDescription()
    {
        var request = Valid();
        request.Description = new string('a', 2001);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_StartingPriceNotPositive_FailsOnStartingPrice(decimal price)
    {
        var request = Valid();
        request.StartingPrice = price;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.StartingPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_MinBidIncrementNotPositive_FailsOnMinBidIncrement(decimal increment)
    {
        var request = Valid();
        request.MinBidIncrement = increment;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.MinBidIncrement);
    }

    [Fact]
    public void Validate_EndAtBeforeStartAt_FailsWithTheOrderMessage()
    {
        var request = Valid();
        request.StartAt = DateTime.UtcNow.AddDays(7);
        request.EndAt = DateTime.UtcNow.AddDays(1);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.EndAt).WithErrorMessage("End time must be after start time.");
    }

    [Fact]
    public void Validate_EndAtEqualsStartAt_Fails()
    {
        var request = Valid();
        var same = DateTime.UtcNow.AddDays(1);
        request.StartAt = same;
        request.EndAt = same;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.EndAt);
    }

    [Fact]
    public void Validate_EndAtInThePast_FailsWithTheFutureMessage()
    {
        var request = Valid();
        request.StartAt = DateTime.UtcNow.AddDays(-2);
        request.EndAt = DateTime.UtcNow.AddDays(-1);

        var result = _validator.TestValidate(request);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.EndAt) && e.ErrorMessage == "End time must be in the future.");
    }
}
