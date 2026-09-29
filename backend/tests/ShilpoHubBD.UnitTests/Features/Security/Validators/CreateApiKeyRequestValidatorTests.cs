using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Validators.Security;

namespace ShilpoHubBD.UnitTests.Features.Security.Validators;

[Trait("Feature", "Security")]
[Trait("Layer", "Validator")]
public class CreateApiKeyRequestValidatorTests
{
    private readonly CreateApiKeyRequestValidator _validator = new();

    [Fact]
    public void Validate_NamedKeyWithoutExpiry_HasNoErrors()
        => _validator.TestValidate(new CreateApiKeyRequest { Name = "Reporting integration" }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_NamedKeyWithExpiry_HasNoErrors()
        => _validator.TestValidate(new CreateApiKeyRequest { Name = "Temp", ExpiresAt = DateTime.UtcNow.AddDays(30) })
            .ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NameMissing_FailsOnName(string name)
        => _validator.TestValidate(new CreateApiKeyRequest { Name = name }).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Validate_NameAtMaximumLength_Passes()
        => _validator.TestValidate(new CreateApiKeyRequest { Name = new string('k', 200) }).ShouldNotHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Validate_NameOverMaximumLength_FailsOnName()
        => _validator.TestValidate(new CreateApiKeyRequest { Name = new string('k', 201) }).ShouldHaveValidationErrorFor(x => x.Name);
}
