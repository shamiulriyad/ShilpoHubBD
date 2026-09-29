using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Auction;
using ShilpoHubBD.Application.Validators.Auction;
using ShilpoHubBD.Domain.Entities.Auction;

namespace ShilpoHubBD.UnitTests.Features.Auction.Validators;

[Trait("Feature", "Auction")]
[Trait("Layer", "Validator")]
public class AuctionQueryParametersValidatorTests
{
    private readonly AuctionQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Defaults_HaveNoErrors()
        => _validator.TestValidate(new AuctionQueryParameters()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_NoStatusFilter_HasNoErrors()
        => _validator.TestValidate(new AuctionQueryParameters { Status = null }).ShouldNotHaveValidationErrorFor(x => x.Status);

    [Fact]
    public void Validate_KnownStatus_HasNoErrors()
        => _validator.TestValidate(new AuctionQueryParameters { Status = AuctionStatus.Active }).ShouldNotHaveValidationErrorFor(x => x.Status);

    [Fact]
    public void Validate_UnknownEnumValue_FailsOnStatus()
        => _validator.TestValidate(new AuctionQueryParameters { Status = (AuctionStatus)999 }).ShouldHaveValidationErrorFor(x => x.Status);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageBelowOne_FailsOnPage(int page)
        => _validator.TestValidate(new AuctionQueryParameters { Page = page }).ShouldHaveValidationErrorFor(x => x.Page);

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_PageSizeOutsideOneToFifty_FailsOnPageSize(int pageSize)
        => _validator.TestValidate(new AuctionQueryParameters { PageSize = pageSize }).ShouldHaveValidationErrorFor(x => x.PageSize);

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Validate_PageSizeAtTheBounds_Passes(int pageSize)
        => _validator.TestValidate(new AuctionQueryParameters { PageSize = pageSize }).ShouldNotHaveValidationErrorFor(x => x.PageSize);
}
