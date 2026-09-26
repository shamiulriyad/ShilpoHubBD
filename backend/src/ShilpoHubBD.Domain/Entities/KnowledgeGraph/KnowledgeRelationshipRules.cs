namespace ShilpoHubBD.Domain.Entities.KnowledgeGraph;

public sealed record KnowledgeRelationshipRule(
    KnowledgeNodeType SourceType,
    KnowledgeRelationshipType RelationshipType,
    KnowledgeNodeType TargetType,
    string ReverseLabel);

/// <summary>Deterministic graph schema. These rules validate every stored edge; no inference is used.</summary>
public static class KnowledgeRelationshipRules
{
    public static readonly IReadOnlyList<KnowledgeRelationshipRule> All = new[]
    {
        R(KnowledgeNodeType.Artisan, KnowledgeRelationshipType.Creates, KnowledgeNodeType.Product, "CREATED_BY"),
        R(KnowledgeNodeType.Artisan, KnowledgeRelationshipType.Practices, KnowledgeNodeType.Craft, "PRACTICED_BY"),
        R(KnowledgeNodeType.Artisan, KnowledgeRelationshipType.LivesIn, KnowledgeNodeType.Village, "HOME_OF"),
        R(KnowledgeNodeType.Producer, KnowledgeRelationshipType.Produces, KnowledgeNodeType.Product, "PRODUCED_BY"),
        R(KnowledgeNodeType.Producer, KnowledgeRelationshipType.OperatesIn, KnowledgeNodeType.Village, "HAS_PRODUCER"),
        R(KnowledgeNodeType.Producer, KnowledgeRelationshipType.SpecializesIn, KnowledgeNodeType.Heritage, "SUPPORTED_BY"),
        R(KnowledgeNodeType.Product, KnowledgeRelationshipType.MadeFrom, KnowledgeNodeType.Material, "USED_IN"),
        R(KnowledgeNodeType.Product, KnowledgeRelationshipType.BelongsTo, KnowledgeNodeType.Heritage, "HAS_PRODUCT"),
        R(KnowledgeNodeType.Product, KnowledgeRelationshipType.UsesTechnique, KnowledgeNodeType.Technique, "USED_BY"),
        R(KnowledgeNodeType.Product, KnowledgeRelationshipType.VariantOf, KnowledgeNodeType.Product, "HAS_VARIANT"),
        R(KnowledgeNodeType.Craft, KnowledgeRelationshipType.UsesMaterial, KnowledgeNodeType.Material, "USED_BY"),
        R(KnowledgeNodeType.Craft, KnowledgeRelationshipType.UsesTechnique, KnowledgeNodeType.Technique, "USED_BY"),
        R(KnowledgeNodeType.Craft, KnowledgeRelationshipType.EvolvedFrom, KnowledgeNodeType.Craft, "EVOLVED_INTO"),
        R(KnowledgeNodeType.Material, KnowledgeRelationshipType.SourcedFrom, KnowledgeNodeType.District, "SOURCE_OF"),
        R(KnowledgeNodeType.Village, KnowledgeRelationshipType.LocatedIn, KnowledgeNodeType.District, "CONTAINS"),
        R(KnowledgeNodeType.Village, KnowledgeRelationshipType.KnownFor, KnowledgeNodeType.Craft, "KNOWN_IN"),
        R(KnowledgeNodeType.Village, KnowledgeRelationshipType.KnownFor, KnowledgeNodeType.Food, "KNOWN_IN"),
        R(KnowledgeNodeType.Village, KnowledgeRelationshipType.Near, KnowledgeNodeType.TouristPlace, "NEAR"),
        R(KnowledgeNodeType.Heritage, KnowledgeRelationshipType.OriginatesFrom, KnowledgeNodeType.Village, "ORIGIN_OF"),
        R(KnowledgeNodeType.Heritage, KnowledgeRelationshipType.AssociatedWith, KnowledgeNodeType.Community, "ASSOCIATED_HERITAGE"),
        R(KnowledgeNodeType.Heritage, KnowledgeRelationshipType.RelatedTo, KnowledgeNodeType.Heritage, "RELATED_TO"),
        R(KnowledgeNodeType.Heritage, KnowledgeRelationshipType.ParentOf, KnowledgeNodeType.Heritage, "CHILD_OF"),
        R(KnowledgeNodeType.Heritage, KnowledgeRelationshipType.SubtypeOf, KnowledgeNodeType.Heritage, "HAS_SUBTYPE"),
        R(KnowledgeNodeType.Heritage, KnowledgeRelationshipType.BelongsToCategory, KnowledgeNodeType.HeritageCategory, "HAS_HERITAGE"),
        R(KnowledgeNodeType.TouristPlace, KnowledgeRelationshipType.LocatedIn, KnowledgeNodeType.Village, "CONTAINS"),
        R(KnowledgeNodeType.TouristPlace, KnowledgeRelationshipType.LocatedIn, KnowledgeNodeType.District, "CONTAINS"),
        R(KnowledgeNodeType.Festival, KnowledgeRelationshipType.CelebratedIn, KnowledgeNodeType.District, "HOSTS"),
        R(KnowledgeNodeType.Food, KnowledgeRelationshipType.AssociatedWith, KnowledgeNodeType.District, "KNOWN_FOR"),
        R(KnowledgeNodeType.CulturalSite, KnowledgeRelationshipType.Represents, KnowledgeNodeType.Heritage, "REPRESENTED_BY"),
        R(KnowledgeNodeType.Craft, KnowledgeRelationshipType.PartOf, KnowledgeNodeType.CulturalTradition, "HAS_CRAFT"),
    };

    public static bool IsValid(KnowledgeNodeType source, KnowledgeRelationshipType relation, KnowledgeNodeType target) =>
        All.Any(rule => rule.SourceType == source && rule.RelationshipType == relation && rule.TargetType == target);

    private static KnowledgeRelationshipRule R(KnowledgeNodeType source, KnowledgeRelationshipType relation,
        KnowledgeNodeType target, string reverse) => new(source, relation, target, reverse);
}
