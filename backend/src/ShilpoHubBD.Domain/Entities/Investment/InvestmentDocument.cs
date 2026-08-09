namespace ShilpoHubBD.Domain.Entities.Investment;

public class InvestmentDocument
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public InvestmentProposal Proposal { get; set; } = null!;

    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
