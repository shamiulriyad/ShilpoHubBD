using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.DTOs.BusinessPartner;

public class BusinessPartnerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

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
    public string? DistrictName { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;

    public string ContactPersonName { get; set; } = string.Empty;
    public string? ContactPersonDesignation { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public int? MinimumOrderQuantity { get; set; }
    public decimal? MaxBudgetPerOrder { get; set; }
    public ProcurementOrderFrequency? PreferredOrderFrequency { get; set; }
    public string? PreferredPaymentTerms { get; set; }
    public List<Guid> PreferredCategoryIds { get; set; } = new();

    public BusinessVerificationStatus VerificationStatus { get; set; }
    public string? VerifiedByName { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public List<BusinessDocumentDto> Documents { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
