using FluentValidation.TestHelper;
using ShilpoHubBD.Application.DTOs.Complaints;
using ShilpoHubBD.Application.Validators.Complaints;

namespace ShilpoHubBD.UnitTests.Features.Complaints.Validators;

[Trait("Feature", "Complaints")]
[Trait("Layer", "Validator")]
public class CustomerComplaintNoteRequestValidatorTests
{
    private readonly CustomerComplaintNoteRequestValidator _validator = new();

    [Fact]
    public void Validate_NullNote_HasNoErrors()
        => _validator.TestValidate(new CustomerComplaintNoteRequest { Note = null }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_NoteTooLong_FailsOnNote()
        => _validator.TestValidate(new CustomerComplaintNoteRequest { Note = new string('a', 1001) }).ShouldHaveValidationErrorFor(x => x.Note);

    [Fact]
    public void Validate_NoteAtMaxLength_HasNoError()
        => _validator.TestValidate(new CustomerComplaintNoteRequest { Note = new string('a', 1000) }).ShouldNotHaveValidationErrorFor(x => x.Note);
}
