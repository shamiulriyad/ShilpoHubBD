using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.Services.ProductDevelopment;

public class ProductDevelopmentService : IProductDevelopmentService
{
    private readonly IProductDevelopmentRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDistrictRepository _districtRepository;

    public ProductDevelopmentService(
        IProductDevelopmentRepository repository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IDistrictRepository districtRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _districtRepository = districtRepository;
    }

    public async Task<DevelopmentProjectDto> CreateAsync(Guid businessPartnerId, CreateDevelopmentProjectRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var now = DateTime.UtcNow;
        var project = new ProductDevelopmentProject
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ProducerId = request.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            BusinessRequirements = request.BusinessRequirements.Trim(),
            ProductSpecifications = request.ProductSpecifications.Trim(),
            Status = DevelopmentStatus.Requested,
            CreatedAt = now,
            UpdatedAt = now,
        };

        project.StatusHistory.Add(new ProductDevelopmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = DevelopmentStatus.Requested,
            Note = "Producer invited to collaborate on development.",
            CreatedAt = now,
        });

        await _repository.AddAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetByIdWithDetailsAsync(project.Id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");
        return ToDto(created);
    }

    public async Task<PagedResult<DevelopmentProjectListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _repository.GetPagedAllAsync(parameters, cancellationToken)
            : await _repository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<DevelopmentProjectListItemDto>> GetForProducerAsync(
        Guid producerId, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<DevelopmentProjectDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToDto(project);
    }

    public async Task<DevelopmentProjectDto> RespondAsync(Guid id, Guid producerId, DevelopmentResponseRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");

        if (project.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to respond to this project.");
        }

        if (project.Status != DevelopmentStatus.Requested)
        {
            throw new ConflictException("Only a requested project can be responded to.");
        }

        var now = DateTime.UtcNow;
        var newStatus = request.Accept ? DevelopmentStatus.Active : DevelopmentStatus.Declined;

        project.Status = newStatus;
        project.RespondedAt = now;
        project.UpdatedAt = now;
        project.StatusHistory.Add(new ProductDevelopmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = newStatus,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    public async Task<DevelopmentCommentDto> AddCommentAsync(
        Guid id, Guid currentUserId, bool isAdmin, AddDevelopmentCommentRequest request, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        var author = await _userRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var comment = new ProductDevelopmentComment
        {
            Id = Guid.NewGuid(),
            AuthorUserId = currentUserId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        project.Comments.Add(comment);
        project.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return new DevelopmentCommentDto
        {
            Id = comment.Id,
            AuthorUserId = currentUserId,
            AuthorName = author.FullName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
        };
    }

    public async Task<DevelopmentMilestoneDto> AddMilestoneAsync(
        Guid id, Guid currentUserId, bool isAdmin, DevelopmentMilestoneInput request, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        var milestone = new ProductDevelopmentMilestone
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDate = request.DueDate,
            Status = DevelopmentMilestoneStatus.Pending,
            DisplayOrder = project.Milestones.Count,
        };

        project.Milestones.Add(milestone);
        project.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task<DevelopmentMilestoneDto> UpdateMilestoneStatusAsync(
        Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateDevelopmentMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        var milestone = project.Milestones.FirstOrDefault(m => m.Id == milestoneId)
            ?? throw new NotFoundException("Milestone not found.");

        milestone.Status = request.Status;
        milestone.CompletedAt = request.Status == DevelopmentMilestoneStatus.Completed ? DateTime.UtcNow : null;
        project.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task<PrototypeVersionDto> SubmitPrototypeAsync(
        Guid id, Guid producerId, SubmitPrototypeRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");

        if (project.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to submit a prototype for this project.");
        }

        if (project.Status != DevelopmentStatus.Active)
        {
            throw new ConflictException("Prototypes can only be submitted to an active project.");
        }

        var now = DateTime.UtcNow;
        var version = new PrototypeVersion
        {
            Id = Guid.NewGuid(),
            VersionNumber = project.PrototypeVersions.Count + 1,
            Description = request.Description.Trim(),
            Status = PrototypeStatus.Pending,
            SubmittedByUserId = producerId,
            SubmittedAt = now,
        };

        foreach (var file in request.Files)
        {
            version.Files.Add(new PrototypeFile
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName.Trim(),
                FileUrl = file.FileUrl.Trim(),
                FileType = file.FileType.Trim(),
                UploadedAt = now,
            });
        }

        project.PrototypeVersions.Add(version);
        project.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);

        version.SubmittedBy = project.Producer;
        return ToPrototypeDto(version);
    }

    public async Task<PrototypeVersionDto> DecidePrototypeAsync(
        Guid id, Guid prototypeVersionId, Guid businessPartnerId, bool isAdmin, PrototypeDecisionRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");

        if (!isAdmin && project.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to decide on this prototype.");
        }

        var version = project.PrototypeVersions.FirstOrDefault(v => v.Id == prototypeVersionId)
            ?? throw new NotFoundException("Prototype version not found.");

        if (version.Status != PrototypeStatus.Pending)
        {
            throw new ConflictException("This prototype version has already been decided.");
        }

        var now = DateTime.UtcNow;
        version.Status = request.Status;
        version.DecidedAt = now;
        version.DecisionNotes = string.IsNullOrWhiteSpace(request.DecisionNotes) ? null : request.DecisionNotes.Trim();
        project.UpdatedAt = now;

        if (request.Status == PrototypeStatus.Approved)
        {
            project.Status = DevelopmentStatus.Approved;
            project.ApprovedAt = now;
            project.StatusHistory.Add(new ProductDevelopmentStatusEvent
            {
                Id = Guid.NewGuid(),
                Status = DevelopmentStatus.Approved,
                Note = $"Prototype version {version.VersionNumber} approved.",
                CreatedAt = now,
            });
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToPrototypeDto(version);
    }

    public async Task<DevelopmentProjectDto> ConvertToProductAsync(
        Guid id, Guid businessPartnerId, bool isAdmin, ConvertToProductRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");

        if (!isAdmin && project.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to convert this project.");
        }

        if (project.Status != DevelopmentStatus.Approved)
        {
            throw new ConflictException("Only a project with an approved prototype can be converted into a product.");
        }

        if (await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken) is null)
        {
            throw new NotFoundException("Category not found.");
        }

        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = project.Title.Trim(),
            Slug = await GenerateUniqueSlugAsync(project.Title, cancellationToken),
            Description = project.ProductSpecifications,
            Price = request.Price,
            Stock = request.InitialStock,
            CategoryId = request.CategoryId,
            DistrictId = request.DistrictId,
            ProducerId = project.ProducerId,
            IsActive = false, // Producer/BP can activate the listing once ready to sell.
            IsFeatured = false,
            HandmadeVerificationStatus = HandmadeVerificationStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _productRepository.AddAsync(product, cancellationToken);

        project.Status = DevelopmentStatus.Converted;
        project.FinalProductId = product.Id;
        project.UpdatedAt = now;
        project.StatusHistory.Add(new ProductDevelopmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = DevelopmentStatus.Converted,
            Note = $"Converted into catalog product '{product.Name}'.",
            CreatedAt = now,
        });

        // The new Product and the project mutations share this scoped DbContext, so one
        // SaveChanges commits both.
        await _repository.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetByIdWithDetailsAsync(project.Id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");
        return ToDto(updated);
    }

    public async Task<DevelopmentProjectDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (project.Status is DevelopmentStatus.Converted or DevelopmentStatus.Cancelled or DevelopmentStatus.Declined)
        {
            throw new ConflictException("This project can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        project.Status = DevelopmentStatus.Cancelled;
        project.UpdatedAt = now;
        project.StatusHistory.Add(new ProductDevelopmentStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = DevelopmentStatus.Cancelled,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    private async Task<ProductDevelopmentProject> GetPartyAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product development project not found.");

        if (!isAdmin && project.BusinessPartnerId != currentUserId && project.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this project.");
        }

        return project;
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string referenceNumber;

        do
        {
            referenceNumber = $"PDV-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _repository.ExistsByReferenceNumberAsync(referenceNumber, cancellationToken));

        return referenceNumber;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _productRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static PagedResult<DevelopmentProjectListItemDto> ToPagedListDto(
        List<ProductDevelopmentProject> items, int totalCount, DevelopmentProjectQueryParameters parameters)
    {
        return new PagedResult<DevelopmentProjectListItemDto>
        {
            Items = items.Select(p => new DevelopmentProjectListItemDto
            {
                Id = p.Id,
                ReferenceNumber = p.ReferenceNumber,
                Title = p.Title,
                ProducerName = p.Producer.FullName,
                Status = p.Status,
                PrototypeVersionCount = p.PrototypeVersions.Count,
                CreatedAt = p.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static PrototypeFileDto ToFileDto(PrototypeFile file) => new()
    {
        Id = file.Id,
        FileName = file.FileName,
        FileUrl = file.FileUrl,
        FileType = file.FileType,
        UploadedAt = file.UploadedAt,
    };

    private static PrototypeVersionDto ToPrototypeDto(PrototypeVersion version) => new()
    {
        Id = version.Id,
        VersionNumber = version.VersionNumber,
        Description = version.Description,
        Status = version.Status,
        SubmittedByUserId = version.SubmittedByUserId,
        SubmittedByName = version.SubmittedBy?.FullName ?? string.Empty,
        SubmittedAt = version.SubmittedAt,
        DecidedAt = version.DecidedAt,
        DecisionNotes = version.DecisionNotes,
        Files = version.Files.Select(ToFileDto).ToList(),
    };

    private static DevelopmentMilestoneDto ToMilestoneDto(ProductDevelopmentMilestone milestone) => new()
    {
        Id = milestone.Id,
        Title = milestone.Title,
        Description = milestone.Description,
        DueDate = milestone.DueDate,
        Status = milestone.Status,
        CompletedAt = milestone.CompletedAt,
        DisplayOrder = milestone.DisplayOrder,
    };

    private static DevelopmentProjectDto ToDto(ProductDevelopmentProject project) => new()
    {
        Id = project.Id,
        ReferenceNumber = project.ReferenceNumber,
        BusinessPartnerId = project.BusinessPartnerId,
        BusinessPartnerName = project.BusinessPartner.FullName,
        ProducerId = project.ProducerId,
        ProducerName = project.Producer.FullName,
        Title = project.Title,
        BusinessRequirements = project.BusinessRequirements,
        ProductSpecifications = project.ProductSpecifications,
        Status = project.Status,
        RespondedAt = project.RespondedAt,
        ApprovedAt = project.ApprovedAt,
        FinalProductId = project.FinalProductId,
        FinalProductName = project.FinalProduct?.Name,
        FinalProductSlug = project.FinalProduct?.Slug,
        PrototypeVersions = project.PrototypeVersions.OrderBy(v => v.VersionNumber).Select(ToPrototypeDto).ToList(),
        Comments = project.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new DevelopmentCommentDto
            {
                Id = c.Id,
                AuthorUserId = c.AuthorUserId,
                AuthorName = c.Author.FullName,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
            }).ToList(),
        Milestones = project.Milestones.OrderBy(m => m.DisplayOrder).Select(ToMilestoneDto).ToList(),
        StatusHistory = project.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new DevelopmentStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
    };
}
