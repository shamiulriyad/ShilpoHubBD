namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class ConvertToProductRequest
{
    public Guid CategoryId { get; set; }
    public Guid DistrictId { get; set; }
    public decimal Price { get; set; }
    public int InitialStock { get; set; }
}
