using FluentValidation;
using ShilpoHubBD.Application.DTOs.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;

namespace ShilpoHubBD.Application.Validators.KnowledgeGraph;

public class CreateKnowledgeRelationshipRequestValidator : AbstractValidator<CreateKnowledgeRelationshipRequest>
{
    public CreateKnowledgeRelationshipRequestValidator()
    {
        RuleFor(x => x.SourceNodeId).NotEmpty();
        RuleFor(x => x.TargetNodeId).NotEmpty();
        RuleFor(x => x.TargetNodeId)
            .NotEqual(x => x.SourceNodeId)
            .WithMessage("A relationship cannot connect a node to itself.");
        RuleFor(x => x.Weight).InclusiveBetween(0, 1_000_000).When(x => x.Weight.HasValue);
        RuleFor(x => x.Label).MaximumLength(200);
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.MetadataJson).MaximumLength(8000);
        RuleFor(x => x.RelationshipType)
            .NotEmpty()
            .Must(t => Enum.TryParse<KnowledgeRelationshipType>(t, true, out _))
            .WithMessage("RelationshipType is not a valid knowledge relationship type.");
    }
}
