using ShilpoHubBD.Application.DTOs.ProductDevelopment;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProductDevelopmentRepository
{
    Task<ProductDevelopmentProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<ProductDevelopmentProject> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ProductDevelopmentProject> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ProductDevelopmentProject> Items, int TotalCount)> GetPagedAllAsync(DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);
    Task AddAsync(ProductDevelopmentProject project, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
