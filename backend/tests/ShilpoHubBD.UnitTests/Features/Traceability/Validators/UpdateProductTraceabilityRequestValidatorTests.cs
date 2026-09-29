using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Validators.Traceability;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Validators;

[Trait("Feature", "Traceability")]
[Trait("Layer", "Validator")]
public class UpdateProductTraceabilityRequestValidatorTests
{
    private readonly UpdateProductTraceabilityRequestValidator _validator = new();

    private static UpdateProductTraceabilityRequest Valid() => new() { Summary = "Updated summary." };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptySummary_FailsOnSummary()
    {
        var request = Valid();
        request.Summary = "";
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Summary);
    }

    [Fact]
    public void Validate_SummaryTooLong_FailsOnSummary()
    {
        var request = Valid();
        request.Summary = new string('a', 2001);
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Summary);
    }

    [Fact]
    public void Validate_InvalidMaterialSource_FailsOnThatItem()
    {
        var request = Valid();
        request.MaterialSources = [new MaterialSourceInput { MaterialName = "Cotton", SourceLocation = "", Description = "x" }];

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("MaterialSources[0].SourceLocation");
    }

    [Fact]
    public void Validate_InvalidTimelineEvent_FailsOnThatItem()
    {
        var request = Valid();
        request.TimelineEvents = [new TimelineEventInput { Title = "Woven", Description = "", EventDate = DateTime.UtcNow }];

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("TimelineEvents[0].Description");
    }
}
