namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>
/// Neutral 5-band rating of a computed score. Interpretation depends on the index: for the risk /
/// climate indices a high score is bad, for the health / participation indices a high score is good.
/// The record's <c>Summary</c> spells out the direction.
/// </summary>
public enum HeritageIndexRating
{
    Critical,
    Poor,
    Fair,
    Good,
    Strong,
}
