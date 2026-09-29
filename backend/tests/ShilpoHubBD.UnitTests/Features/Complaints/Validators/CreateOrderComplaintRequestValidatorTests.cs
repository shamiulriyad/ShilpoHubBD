using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Validators.Complaints;

namespace ShilpoHubBD.UnitTests.Features.Complaints.Validators;

[Trait("Feature", "Complaints")]
[Trait("Layer", "Validator")]
public class CreateOrderComplaintRequestValidatorTests
{
    private readonly CreateOrderComplaintRequestValidator _validator = new();

    private static CreateOrderComplaintRequest Valid() => new()
    {
        OrderId = Guid.NewGuid(),
        ProductId = Guid.NewGuid(),
        Subject = "Broken item",
        Description = "It arrived cracked.",
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyOrderId_FailsOnOrderId()
    {
        var request = Valid();
        request.OrderId = Guid.Empty;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void Validate_EmptyProductId_FailsOnProductId()
    {
        var request = Valid();
        request.ProductId = Guid.Empty;
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Validate_EmptySubject_FailsOnSubject()
    {
        var request = Valid();
        request.Subject = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Subject);
    }

    [Fact]
    public void Validate_SubjectTooLong_FailsOnSubject()
    {
        var request = Valid();
        request.Subject = new string('a', 201);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Subject);
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
        request.Description = new string('a', 2001);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullImageUrl_HasNoError()
    {
        var request = Valid();
        request.ImageUrl = null;
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ImageUrlNotUnderUploads_FailsOnImageUrl()
    {
        var request = Valid();
        request.ImageUrl = "https://evil.example.com/x.png";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validate_ImageUrlUnderUploads_HasNoError()
    {
        var request = Valid();
        request.ImageUrl = "/uploads/chat/x.png";
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }
}
