using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly ICraftStoryRepository _craftStoryRepository;
    private readonly IProducerStoryRepository _producerStoryRepository;
    private readonly IUserRepository _userRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IDistrictRepository districtRepository,
        ICraftStoryRepository craftStoryRepository,
        IProducerStoryRepository producerStoryRepository,
        IUserRepository userRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _districtRepository = districtRepository;
        _craftStoryRepository = craftStoryRepository;
        _producerStoryRepository = producerStoryRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<ProductListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        product.ViewCount++;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    public async Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        product.ViewCount++;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    public async Task<List<ProductListItemDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetFeaturedAsync(Math.Clamp(count, 1, 50), cancellationToken);
        return products.Select(ToListItemDto).ToList();
    }

    public async Task<List<ProductListItemDto>> GetTrendingAsync(int count, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetTrendingAsync(Math.Clamp(count, 1, 50), cancellationToken);
        return products.Select(ToListItemDto).ToList();
    }

    public async Task<List<ProductDto>> GetMineAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetByProducerAsync(producerId, cancellationToken);

        var result = new List<ProductDto>();
        foreach (var product in products)
        {
            result.Add(await ToDtoAsync(product, cancellationToken));
        }

        return result;
    }

    public async Task<ProductDto> CreateAsync(Guid producerId, CreateProductRequest request, CancellationToken cancellationToken)
    {
        await EnsureCategoryAndDistrictExistAsync(request.CategoryId, request.DistrictId, cancellationToken);

        var slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
        var now = DateTime.UtcNow;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            Stock = request.Stock,
            LowStockThreshold = request.LowStockThreshold,
            CategoryId = request.CategoryId,
            DistrictId = request.DistrictId,
            ProducerId = producerId,
            IsActive = true,
            IsFeatured = false,
            MakingProcessVideoUrl = request.MakingProcessVideoUrl?.Trim(),
            Story = string.IsNullOrWhiteSpace(request.Story) ? null : request.Story.Trim(),
            HandmadeVerificationStatus = HandmadeVerificationStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };

        AttachImages(product, request.ImageUrls, request.ThreeSixtyImageUrls);

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        var created = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        return await ToDtoAsync(created!, cancellationToken);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, Guid currentUserId, bool isAdmin, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        await EnsureCategoryAndDistrictExistAsync(request.CategoryId, request.DistrictId, cancellationToken);

        if (!product.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            product.Slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken);
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Stock = request.Stock;
        product.LowStockThreshold = request.LowStockThreshold;
        product.CategoryId = request.CategoryId;
        product.DistrictId = request.DistrictId;
        product.IsActive = request.IsActive;
        product.MakingProcessVideoUrl = request.MakingProcessVideoUrl?.Trim();
        product.Story = string.IsNullOrWhiteSpace(request.Story) ? null : request.Story.Trim();
        product.UpdatedAt = DateTime.UtcNow;

        product.Images.Clear();
        AttachImages(product, request.ImageUrls, request.ThreeSixtyImageUrls);

        await _productRepository.SaveChangesAsync(cancellationToken);

        var updated = await _productRepository.GetByIdAsync(id, cancellationToken);
        return await ToDtoAsync(updated!, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDto> SetFeaturedAsync(Guid id, bool isFeatured, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        product.IsFeatured = isFeatured;
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    public async Task<ProductDto> AddVariantAsync(Guid productId, Guid currentUserId, bool isAdmin, CreateProductVariantRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = request.Name.Trim(),
            Sku = request.Sku?.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
        };

        await _productRepository.AddVariantAsync(variant, cancellationToken);

        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        var updated = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return await ToDtoAsync(updated!, cancellationToken);
    }

    public async Task<ProductDto> UpdateVariantAsync(Guid productId, Guid variantId, Guid currentUserId, bool isAdmin, UpdateProductVariantRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new NotFoundException("Variant not found.");

        variant.Name = request.Name.Trim();
        variant.Sku = request.Sku?.Trim();
        variant.Price = request.Price;
        variant.Stock = request.Stock;
        variant.DisplayOrder = request.DisplayOrder;
        variant.IsActive = request.IsActive;

        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    public async Task<ProductDto> DeleteVariantAsync(Guid productId, Guid variantId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new NotFoundException("Variant not found.");

        product.Variants.Remove(variant);
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    private static void EnsureOwnership(Product product, Guid currentUserId, bool isAdmin)
    {
        if (!isAdmin && product.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this product.");
        }
    }

    private async Task EnsureCategoryAndDistrictExistAsync(Guid categoryId, Guid districtId, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.GetByIdAsync(categoryId, cancellationToken) is null)
        {
            throw new NotFoundException("Category not found.");
        }

        if (await _districtRepository.GetByIdAsync(districtId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }
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

    private static void AttachImages(Product product, List<string> imageUrls, List<string> threeSixtyImageUrls)
    {
        for (var i = 0; i < imageUrls.Count; i++)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = imageUrls[i].Trim(),
                DisplayOrder = i,
                IsPrimary = i == 0,
                ImageType = ProductImageType.Standard,
            });
        }

        for (var i = 0; i < threeSixtyImageUrls.Count; i++)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = threeSixtyImageUrls[i].Trim(),
                DisplayOrder = i,
                IsPrimary = false,
                ImageType = ProductImageType.ThreeSixty,
            });
        }
    }

    public async Task<BulkCreateProductsResultDto> BulkCreateAsync(Guid producerId, BulkCreateProductsRequest request, CancellationToken cancellationToken)
    {
        var result = new BulkCreateProductsResultDto();

        for (var i = 0; i < request.Products.Count; i++)
        {
            var item = request.Products[i];
            try
            {
                var created = await CreateAsync(producerId, item, cancellationToken);
                result.Created.Add(created);
            }
            catch (NotFoundException ex)
            {
                result.Errors.Add(new BulkProductErrorDto
                {
                    Index = i,
                    ProductName = item.Name,
                    Error = ex.Message,
                });
            }
        }

        return result;
    }

    public async Task<ProductDto> AddVideoAsync(Guid productId, Guid currentUserId, bool isAdmin, CreateProductVideoRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        var video = new ProductVideo
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            VideoUrl = request.VideoUrl.Trim(),
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
        };

        await _productRepository.AddVideoAsync(video, cancellationToken);

        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        var updated = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return await ToDtoAsync(updated!, cancellationToken);
    }

    public async Task<ProductDto> UpdateVideoAsync(Guid productId, Guid videoId, Guid currentUserId, bool isAdmin, UpdateProductVideoRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        var video = product.Videos.FirstOrDefault(v => v.Id == videoId)
            ?? throw new NotFoundException("Video not found.");

        video.VideoUrl = request.VideoUrl.Trim();
        video.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        video.DisplayOrder = request.DisplayOrder;

        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    public async Task<ProductDto> DeleteVideoAsync(Guid productId, Guid videoId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        EnsureOwnership(product, currentUserId, isAdmin);

        var video = product.Videos.FirstOrDefault(v => v.Id == videoId)
            ?? throw new NotFoundException("Video not found.");

        product.Videos.Remove(video);
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(product, cancellationToken);
    }

    public async Task<ProductDto> SetHandmadeVerificationAsync(Guid productId, Guid verifierUserId, SetHandmadeVerificationRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var verifier = await _userRepository.GetByIdAsync(verifierUserId, cancellationToken)
            ?? throw new NotFoundException("Verifying user not found.");

        product.HandmadeVerificationStatus = request.Status;
        product.HandmadeVerifiedByUserId = verifierUserId;
        product.HandmadeVerificationNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        product.HandmadeVerifiedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.SaveChangesAsync(cancellationToken);

        product.HandmadeVerifiedBy = verifier;
        return await ToDtoAsync(product, cancellationToken);
    }

    private static ProductListItemDto ToListItemDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        Price = product.Price,
        DiscountPrice = product.DiscountPrice,
        PrimaryImageUrl = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        CategoryId = product.CategoryId,
        CategoryName = product.Category.Name,
        DistrictId = product.DistrictId,
        DistrictName = product.District.Name,
        ProducerId = product.ProducerId,
        ProducerName = product.Producer.FullName,
        AverageRating = product.AverageRating,
        ReviewCount = product.ReviewCount,
        IsFeatured = product.IsFeatured,
    };

    private async Task<ProductDto> ToDtoAsync(Product product, CancellationToken cancellationToken)
    {
        var hasCraftStory = await _craftStoryRepository.ExistsByCategoryIdAsync(product.CategoryId, cancellationToken);
        var hasProducerStory = await _producerStoryRepository.ExistsByProducerIdAsync(product.ProducerId, cancellationToken);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            Stock = product.Stock,
            LowStockThreshold = product.LowStockThreshold,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            MakingProcessVideoUrl = product.MakingProcessVideoUrl,
            Story = product.Story,
            ViewCount = product.ViewCount,
            SalesCount = product.SalesCount,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            HandmadeVerificationStatus = product.HandmadeVerificationStatus,
            HandmadeVerifiedByName = product.HandmadeVerifiedBy?.FullName,
            HandmadeVerificationNotes = product.HandmadeVerificationNotes,
            HandmadeVerifiedAt = product.HandmadeVerifiedAt,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            DistrictId = product.DistrictId,
            DistrictName = product.District.Name,
            ProducerId = product.ProducerId,
            ProducerName = product.Producer.FullName,
            ImageUrls = product.Images
                .Where(i => i.ImageType == ProductImageType.Standard)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),
            ThreeSixtyImageUrls = product.Images
                .Where(i => i.ImageType == ProductImageType.ThreeSixty)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),
            Variants = product.Variants
                .OrderBy(v => v.DisplayOrder)
                .Select(v => ToVariantDto(v, product.Price))
                .ToList(),
            Videos = product.Videos
                .OrderBy(v => v.DisplayOrder)
                .Select(ToVideoDto)
                .ToList(),
            HasCraftStory = hasCraftStory,
            HasProducerStory = hasProducerStory,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
        };
    }

    private static ProductVariantDto ToVariantDto(ProductVariant variant, decimal basePrice) => new()
    {
        Id = variant.Id,
        Name = variant.Name,
        Sku = variant.Sku,
        Price = variant.Price ?? basePrice,
        Stock = variant.Stock,
        DisplayOrder = variant.DisplayOrder,
        IsActive = variant.IsActive,
    };

    private static ProductVideoDto ToVideoDto(ProductVideo video) => new()
    {
        Id = video.Id,
        VideoUrl = video.VideoUrl,
        Title = video.Title,
        DisplayOrder = video.DisplayOrder,
    };
}
