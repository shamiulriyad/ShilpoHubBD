using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Validators.Security;

namespace ShilpoHubBD.UnitTests.Features.Security.Validators;

[Trait("Feature", "Security")]
[Trait("Layer", "Validator")]
public class BlockIpRequestValidatorTests
{
    private readonly BlockIpRequestValidator _validator = new();

    private static BlockIpRequest Valid() => new() { IpAddress = "203.0.113.7", Reason = "Repeated failed logins." };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_Ipv6Address_HasNoErrors()
    {
        var request = Valid();
        request.IpAddress = "2001:db8::1";

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_IpAddressMissing_FailsOnIpAddress(string ip)
    {
        var request = Valid();
        request.IpAddress = ip;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.IpAddress);
    }

    [Fact]
    public void Validate_IpAddressAtMaximumLength_Passes()
    {
        var request = Valid();
        request.IpAddress = new string('1', 64);

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.IpAddress);
    }

    [Fact]
    public void Validate_IpAddressOverMaximumLength_FailsOnIpAddress()
    {
        var request = Valid();
        request.IpAddress = new string('1', 65);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.IpAddress);
    }

    [Fact]
    public void Validate_TextThatIsNotAnIpAddress_IsAcceptedBecauseTheFormatIsNotChecked()
    {
        var request = Valid();
        request.IpAddress = "not-an-ip";

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.IpAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ReasonMissing_FailsOnReason(string reason)
    {
        var request = Valid();
        request.Reason = reason;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ReasonAtMaximumLength_Passes()
    {
        var request = Valid();
        request.Reason = new string('r', 500);

        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ReasonOverMaximumLength_FailsOnReason()
    {
        var request = Valid();
        request.Reason = new string('r', 501);

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ExpiryIsOptional()
    {
        var request = Valid();
        request.ExpiresAt = null;

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
