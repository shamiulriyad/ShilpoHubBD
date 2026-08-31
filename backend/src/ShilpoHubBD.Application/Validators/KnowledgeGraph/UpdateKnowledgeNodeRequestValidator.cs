using FluentValidation;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;

namespace ShilpoHubBD.Application.Validators.KnowledgeGraph;

public class UpdateKnowledgeNodeRequestValidator : AbstractValidator<UpdateKnowledgeNodeRequest>
{
    public UpdateKnowledgeNodeRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.MetadataJson).MaximumLength(8000);
    }
}
