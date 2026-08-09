using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.DTOs.BusinessPartner;

public class UpsertBusinessPartnerProfileRequest
{
    public BusinessType BusinessType { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? TaxIdentificationNumber { get; set; }
    public int? YearEstablished { get; set; }
    public string Industry { get; set; } = string.Empty;
    public BusinessSize BusinessSize { get; set; }
    public int? EmployeeCount { get; set; }
    public string? Website { get; set; }
    public string CompanyDescription { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Bangladesh";

    public string ContactPersonName { get; set; } = string.Empty;
    public string? ContactPersonDesignation { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public int? MinimumOrderQuantity { get; set; }
    public decimal? MaxBudgetPerOrder { get; set; }
    public ProcurementOrderFrequency? PreferredOrderFrequency { get; set; }
    public string? PreferredPaymentTerms { get; set; }
    public List<Guid> PreferredCategoryIds { get; set; } = new();

    public List<BusinessDocumentInput> Documents { get; set; } = new();
}
