using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.ProducerComparison;
using ShilpoHubBD.Application.Validators.ProducerComparison;

namespace ShilpoHubBD.UnitTests.Features.ProducerComparison.Validators;

[Trait("Feature", "ProducerComparison")]
[Trait("Layer", "Validator")]
public class ProducerComparisonRequestValidatorTests
{
    private readonly ProducerComparisonRequestValidator _validator = new();

    [Fact]
    public void Validate_TwoDistinctProducers_HasNoErrors()
        => _validator.TestValidate(new ProducerComparisonRequest { ProducerIds = [Guid.NewGuid(), Guid.NewGuid()] }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_FiveDistinctProducers_HasNoErrors()
        => _validator.TestValidate(new ProducerComparisonRequest { ProducerIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList() })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_FewerThanTwoDistinctProducers_FailsOnProducerIds()
        => _validator.TestValidate(new ProducerComparisonRequest { ProducerIds = [Guid.NewGuid()] }).ShouldHaveValidationErrorFor(x => x.ProducerIds);

    [Fact]
    public void Validate_DuplicateIdsCountingAsOne_FailsOnProducerIds()
    {
        var id = Guid.NewGuid();
        _validator.TestValidate(new ProducerComparisonRequest { ProducerIds = [id, id] }).ShouldHaveValidationErrorFor(x => x.ProducerIds);
    }

    [Fact]
    public void Validate_MoreThanFiveDistinctProducers_FailsOnProducerIds()
        => _validator.TestValidate(new ProducerComparisonRequest { ProducerIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList() })
            .ShouldHaveValidationErrorFor(x => x.ProducerIds);
}
