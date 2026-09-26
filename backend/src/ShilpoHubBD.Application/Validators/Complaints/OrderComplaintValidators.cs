using FluentValidation;
using ShilpoHubBD.Application.DTOs.Complaints;

namespace ShilpoHubBD.Application.Validators.Complaints;

public class CreateOrderComplaintRequestValidator : AbstractValidator<CreateOrderComplaintRequest>
{
    public CreateOrderComplaintRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(500).Must(u => u is null || u.StartsWith("/uploads/"))
            .WithMessage("Use an image uploaded through the chat image upload.");
    }
}

public class RespondToOrderComplaintRequestValidator : AbstractValidator<RespondToOrderComplaintRequest>
{
    public RespondToOrderComplaintRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public class CustomerComplaintNoteRequestValidator : AbstractValidator<CustomerComplaintNoteRequest>
{
    public CustomerComplaintNoteRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
