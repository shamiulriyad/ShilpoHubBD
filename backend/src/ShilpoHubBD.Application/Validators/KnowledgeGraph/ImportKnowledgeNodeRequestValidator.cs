using FluentValidation;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Validators.KnowledgeGraph;

public class ImportKnowledgeNodeRequestValidator : AbstractValidator<ImportKnowledgeNodeRequest>
{
    public ImportKnowledgeNodeRequestValidator()
    {
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .Must(t => Enum.TryParse<KnowledgeNodeType>(t, true, out _))
            .WithMessage("EntityType is not a valid knowledge node type.");
        RuleFor(x => x.NodeType)
            .NotEmpty()
            .Must(t => Enum.TryParse<KnowledgeNodeType>(t, true, out _))
            .WithMessage("NodeType is not a valid knowledge node type.");
        RuleFor(x => x).Must(x => string.Equals(x.NodeType, x.EntityType, StringComparison.OrdinalIgnoreCase))
            .WithMessage("EntityType must match NodeType.");
    }
}
