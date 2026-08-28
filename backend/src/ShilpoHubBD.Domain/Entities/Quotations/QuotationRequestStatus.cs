namespace ShilpoHubBD.Domain.Entities.Quotations;

public enum QuotationRequestStatus
{
    Sent = 0,
    PartiallyResponded = 1,
    Responded = 2,
    Closed = 3,
    Cancelled = 4,
}
