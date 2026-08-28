using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class DevelopmentProjectQueryParameters
{
    public DevelopmentStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
