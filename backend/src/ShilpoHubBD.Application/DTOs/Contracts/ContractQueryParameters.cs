using ShilpoHubBD.Domain.Entities.Contracts;

namespace ShilpoHubBD.Application.DTOs.Contracts;

public class ContractQueryParameters
{
    public ContractStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
