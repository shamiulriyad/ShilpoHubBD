using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Validators.Traceability;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Validators;

[Trait("Feature", "Traceability")]
[Trait("Layer", "Validator")]
public class CreateProductTraceabilityRequestValidatorTests
{
    private readonly CreateProductTraceabilityRequestValidator _validator = new();

    private static CreateProductTraceabilityRequest Valid() => new() { ProductId = Guid.NewGuid(), Summary = "Handwoven from local cotton." };

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
        request.MaterialSources = [new MaterialSourceInput { MaterialName = "", SourceLocation = "Rangpur", Description = "x" }];

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("MaterialSources[0].MaterialName");
    }

    [Fact]
    public void Validate_InvalidTimelineEvent_FailsOnThatItem()
    {
        var request = Valid();
        request.TimelineEvents = [new TimelineEventInput { Title = "", Description = "x", EventDate = DateTime.UtcNow }];

        _validator.TestValidate(request).ShouldHaveValidationErrorFor("TimelineEvents[0].Title");
    }

    [Fact]
    public void Validate_ValidChildRows_HasNoErrors()
    {
        var request = Valid();
        request.MaterialSources = [new MaterialSourceInput { MaterialName = "Cotton", SourceLocation = "Rangpur", Description = "x" }];
        request.TimelineEvents = [new TimelineEventInput { Title = "Woven", Description = "x", EventDate = DateTime.UtcNow }];

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
