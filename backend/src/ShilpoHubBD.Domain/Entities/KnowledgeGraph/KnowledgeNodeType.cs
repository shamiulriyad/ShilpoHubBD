namespace ShilpoHubBD.Domain.Entities.KnowledgeGraph;

/// <summary>
/// Kind of heritage entity a graph node represents. Producer / Village / Product / HeritagePlace /
/// Family map onto existing platform entities via <c>ExternalEntityId</c>; Craft / Material / Culture
/// / Custom are label-only nodes with no backing row.
/// </summary>
public enum KnowledgeNodeType
{
    Heritage,
    Artisan,
    Producer,
    Village,
    District,
    Division,
    Product,
    Craft,
    Material,
    Technique,
    TouristPlace,
    Food,
    Festival,
    CulturalSite,
    Community,
    CulturalTradition,
    HeritageCategory,
}
