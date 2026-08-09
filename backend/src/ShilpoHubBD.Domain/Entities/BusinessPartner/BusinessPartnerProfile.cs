using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.BusinessPartner;

public class BusinessPartnerProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Business Type / Company Information
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

    // Business Address
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Bangladesh";

    // Contact Information
    public string ContactPersonName { get; set; } = string.Empty;
    public string? ContactPersonDesignation { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    // Procurement Preferences
    public int? MinimumOrderQuantity { get; set; }
    public decimal? MaxBudgetPerOrder { get; set; }
    public ProcurementOrderFrequency? PreferredOrderFrequency { get; set; }
    public string? PreferredPaymentTerms { get; set; }

    // Business Verification
    public BusinessVerificationStatus VerificationStatus { get; set; } = BusinessVerificationStatus.Pending;
    public Guid? VerifiedByUserId { get; set; }
    public User? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<BusinessDocument> Documents { get; set; } = new List<BusinessDocument>();
    public ICollection<BusinessPartnerPreferredCategory> PreferredCategories { get; set; } = new List<BusinessPartnerPreferredCategory>();
}
