using ShilpoHubBD.Application.DTOs.Inventory;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Inventory;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository, IProductRepository productRepository, IUserRepository userRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<InventoryTransactionDto> AdjustStockAsync(
        Guid productId, Guid currentUserId, bool isAdmin, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (!isAdmin && product.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this product's inventory.");
        }

        var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        ProductVariant? variant = null;
        int previousStock;

        if (request.VariantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId.Value)
                ?? throw new NotFoundException("Variant not found.");
            previousStock = variant.Stock;
        }
        else
        {
            previousStock = product.Stock;
        }

        var newStock = previousStock + request.ChangeAmount;
        if (newStock < 0)
        {
            throw new ConflictException("This adjustment would result in negative stock.");
        }

        if (variant is not null)
        {
            variant.Stock = newStock;
        }
        else
        {
            product.Stock = newStock;
        }

        product.UpdatedAt = DateTime.UtcNow;

        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ProductVariantId = variant?.Id,
            ChangeAmount = request.ChangeAmount,
            Reason = request.Reason.Trim(),
            PreviousStock = previousStock,
            NewStock = newStock,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
        };

        await _inventoryRepository.AddAsync(transaction, cancellationToken);
        await _inventoryRepository.SaveChangesAsync(cancellationToken);

        return new InventoryTransactionDto
        {
            Id = transaction.Id,
            ProductId = transaction.ProductId,
            VariantId = transaction.ProductVariantId,
            VariantName = variant?.Name,
            ChangeAmount = transaction.ChangeAmount,
            Reason = transaction.Reason,
            PreviousStock = transaction.PreviousStock,
            NewStock = transaction.NewStock,
            CreatedByName = currentUser.FullName,
            CreatedAt = transaction.CreatedAt,
        };
    }

    public async Task<List<InventoryTransactionDto>> GetHistoryAsync(Guid productId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (!isAdmin && product.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this product's inventory history.");
        }

        var transactions = await _inventoryRepository.GetByProductAsync(productId, cancellationToken);

        return transactions.Select(t => new InventoryTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            VariantId = t.ProductVariantId,
            VariantName = t.ProductVariant?.Name,
            ChangeAmount = t.ChangeAmount,
            Reason = t.Reason,
            PreviousStock = t.PreviousStock,
            NewStock = t.NewStock,
            CreatedByName = t.CreatedBy.FullName,
            CreatedAt = t.CreatedAt,
        }).ToList();
    }

    public async Task<List<LowStockProductDto>> GetLowStockAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetLowStockByProducerAsync(producerId, cancellationToken);

        return products.Select(p => new LowStockProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Stock = p.Stock,
            LowStockThreshold = p.LowStockThreshold!.Value,
            PrimaryImageUrl = p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
        }).ToList();
    }
}
