namespace ShilpoHubBD.Domain.Entities.KnowledgeGraph;

/// <summary>
/// Kind of heritage entity a graph node represents. Producer / Village / Product / HeritagePlace /
/// Family map onto existing platform entities via <c>ExternalEntityId</c>; Craft / Material / Culture
/// / Custom are label-only nodes with no backing row.
/// </summary>
public enum KnowledgeNodeType
{
    Producer,
    Village,
    Product,
    Craft,
    Material,
    Culture,
    Family,
    HeritagePlace,
    Custom,
}
