using FluentValidation;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Validators.KnowledgeGraph;

public class CreateKnowledgeNodeRequestValidator : AbstractValidator<CreateKnowledgeNodeRequest>
{
    public CreateKnowledgeNodeRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.MetadataJson).MaximumLength(8000);
        RuleFor(x => x.NodeType)
            .NotEmpty()
            .Must(t => Enum.TryParse<KnowledgeNodeType>(t, true, out _))
            .WithMessage("NodeType is not a valid knowledge node type.");
    }
}
