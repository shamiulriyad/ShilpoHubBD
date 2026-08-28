using ShilpoHubBD.Application.DTOs.BusinessPartner;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Application.Services.BusinessPartner;

public class BusinessPartnerService : IBusinessPartnerService
{
    private readonly IBusinessPartnerRepository _businessPartnerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BusinessPartnerService(
        IBusinessPartnerRepository businessPartnerRepository,
        IUserRepository userRepository,
        IDistrictRepository districtRepository,
        ICategoryRepository categoryRepository)
    {
        _businessPartnerRepository = businessPartnerRepository;
        _userRepository = userRepository;
        _districtRepository = districtRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<BusinessPartnerProfileDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _businessPartnerRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Business partner profile not found for this user.");

        return ToDto(profile);
    }

    public async Task<PagedResult<BusinessPartnerProfileDto>> GetPagedAsync(
        BusinessPartnerQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _businessPartnerRepository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResult<BusinessPartnerProfileDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    public async Task<BusinessPartnerProfileDto> UpsertAsync(
        Guid userId, Guid currentUserId, bool isAdmin, UpsertBusinessPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        if (!isAdmin && userId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this business partner's profile.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (request.DistrictId.HasValue
            && await _districtRepository.GetByIdAsync(request.DistrictId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        foreach (var categoryId in request.PreferredCategoryIds.Distinct())
        {
            if (await _categoryRepository.GetByIdAsync(categoryId, cancellationToken) is null)
            {
                throw new NotFoundException($"Category '{categoryId}' not found.");
            }
        }

        var profile = await _businessPartnerRepository.GetByUserIdAsync(userId, cancellationToken);

        if (await _businessPartnerRepository.ExistsByRegistrationNumberAsync(
                request.RegistrationNumber, profile?.Id, cancellationToken))
        {
            throw new ConflictException("A business partner profile with this registration number already exists.");
        }

        var now = DateTime.UtcNow;

        if (profile is null)
        {
            profile = new BusinessPartnerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VerificationStatus = BusinessVerificationStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _businessPartnerRepository.AddAsync(profile, cancellationToken);
        }

        profile.BusinessType = request.BusinessType;
        profile.CompanyName = request.CompanyName.Trim();
        profile.RegistrationNumber = request.RegistrationNumber.Trim();
        profile.TaxIdentificationNumber = string.IsNullOrWhiteSpace(request.TaxIdentificationNumber) ? null : request.TaxIdentificationNumber.Trim();
        profile.YearEstablished = request.YearEstablished;
        profile.Industry = request.Industry.Trim();
        profile.BusinessSize = request.BusinessSize;
        profile.EmployeeCount = request.EmployeeCount;
        profile.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        profile.CompanyDescription = request.CompanyDescription.Trim();

        profile.AddressLine = request.AddressLine.Trim();
        profile.City = request.City.Trim();
        profile.DistrictId = request.DistrictId;
        profile.PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim();
        profile.Country = request.Country.Trim();

        profile.ContactPersonName = request.ContactPersonName.Trim();
        profile.ContactPersonDesignation = string.IsNullOrWhiteSpace(request.ContactPersonDesignation) ? null : request.ContactPersonDesignation.Trim();
        profile.ContactPhone = request.ContactPhone.Trim();
        profile.ContactEmail = request.ContactEmail.Trim();

        profile.MinimumOrderQuantity = request.MinimumOrderQuantity;
        profile.MaxBudgetPerOrder = request.MaxBudgetPerOrder;
        profile.PreferredOrderFrequency = request.PreferredOrderFrequency;
        profile.PreferredPaymentTerms = string.IsNullOrWhiteSpace(request.PreferredPaymentTerms) ? null : request.PreferredPaymentTerms.Trim();

        profile.UpdatedAt = now;

        profile.PreferredCategories.Clear();
        foreach (var categoryId in request.PreferredCategoryIds.Distinct())
        {
            profile.PreferredCategories.Add(new BusinessPartnerPreferredCategory
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
            });
        }

        profile.Documents.Clear();
        foreach (var document in request.Documents)
        {
            profile.Documents.Add(new BusinessDocument
            {
                Id = Guid.NewGuid(),
                DocumentType = document.DocumentType,
                DocumentName = document.DocumentName.Trim(),
                FileUrl = document.FileUrl.Trim(),
                DocumentNumber = string.IsNullOrWhiteSpace(document.DocumentNumber) ? null : document.DocumentNumber.Trim(),
                IssuedDate = document.IssuedDate,
                ExpiryDate = document.ExpiryDate,
                UploadedAt = now,
            });
        }

        await _businessPartnerRepository.SaveChangesAsync(cancellationToken);

        profile.User = user;
        return ToDto(profile);
    }

    public async Task<BusinessPartnerProfileDto> VerifyAsync(
        Guid userId, Guid verifierUserId, VerifyBusinessPartnerRequest request, CancellationToken cancellationToken)
    {
        var profile = await _businessPartnerRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Business partner profile not found for this user.");

        var verifier = await _userRepository.GetByIdAsync(verifierUserId, cancellationToken)
            ?? throw new NotFoundException("Verifying user not found.");

        profile.VerificationStatus = request.Status;
        profile.VerifiedByUserId = verifierUserId;
        profile.VerificationNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        profile.VerifiedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _businessPartnerRepository.SaveChangesAsync(cancellationToken);

        profile.VerifiedBy = verifier;
        return ToDto(profile);
    }

    public async Task DeleteAsync(Guid userId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var profile = await _businessPartnerRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Business partner profile not found for this user.");

        if (!isAdmin && profile.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this business partner's profile.");
        }

        _businessPartnerRepository.Remove(profile);
        await _businessPartnerRepository.SaveChangesAsync(cancellationToken);
    }

    private static BusinessPartnerProfileDto ToDto(BusinessPartnerProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        UserFullName = profile.User.FullName,
        UserEmail = profile.User.Email,

        BusinessType = profile.BusinessType,
        CompanyName = profile.CompanyName,
        RegistrationNumber = profile.RegistrationNumber,
        TaxIdentificationNumber = profile.TaxIdentificationNumber,
        YearEstablished = profile.YearEstablished,
        Industry = profile.Industry,
        BusinessSize = profile.BusinessSize,
        EmployeeCount = profile.EmployeeCount,
        Website = profile.Website,
        CompanyDescription = profile.CompanyDescription,

        AddressLine = profile.AddressLine,
        City = profile.City,
        DistrictId = profile.DistrictId,
        DistrictName = profile.District?.Name,
        PostalCode = profile.PostalCode,
        Country = profile.Country,

        ContactPersonName = profile.ContactPersonName,
        ContactPersonDesignation = profile.ContactPersonDesignation,
        ContactPhone = profile.ContactPhone,
        ContactEmail = profile.ContactEmail,

        MinimumOrderQuantity = profile.MinimumOrderQuantity,
        MaxBudgetPerOrder = profile.MaxBudgetPerOrder,
        PreferredOrderFrequency = profile.PreferredOrderFrequency,
        PreferredPaymentTerms = profile.PreferredPaymentTerms,
        PreferredCategoryIds = profile.PreferredCategories.Select(c => c.CategoryId).ToList(),

        VerificationStatus = profile.VerificationStatus,
        VerifiedByName = profile.VerifiedBy?.FullName,
        VerificationNotes = profile.VerificationNotes,
        VerifiedAt = profile.VerifiedAt,

        Documents = profile.Documents.Select(d => new BusinessDocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            DocumentName = d.DocumentName,
            FileUrl = d.FileUrl,
            DocumentNumber = d.DocumentNumber,
            IssuedDate = d.IssuedDate,
            ExpiryDate = d.ExpiryDate,
            UploadedAt = d.UploadedAt,
        }).ToList(),

        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt,
    };
}
