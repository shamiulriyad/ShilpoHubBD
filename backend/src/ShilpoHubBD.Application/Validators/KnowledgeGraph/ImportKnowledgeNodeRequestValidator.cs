using FluentValidation;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Validators.KnowledgeGraph;

public class ImportKnowledgeNodeRequestValidator : AbstractValidator<ImportKnowledgeNodeRequest>
{
    public ImportKnowledgeNodeRequestValidator()
    {
        RuleFor(x => x.ExternalEntityId).NotEmpty();
        RuleFor(x => x.LabelOverride).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.NodeType)
            .NotEmpty()
            .Must(t => Enum.TryParse<KnowledgeNodeType>(t, true, out _))
            .WithMessage("NodeType is not a valid knowledge node type.");
    }
}
