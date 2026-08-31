/**
 * Adapters from backend DTO shapes to the props our presentational
 * components (ProductCard, VillageCard, EntityCard, ...) expect.
 */

// ProductListItemDto / ProductDto -> ProductCard `product` prop
export function mapProduct(dto) {
  if (!dto) return dto;
  return {
    id: dto.id,
    name: dto.name,
    slug: dto.slug,
    category: dto.categoryName,
    producer: dto.producerName,
    district: dto.districtName,
    price: dto.discountPrice ?? dto.price,
    listPrice: dto.price,
    image: dto.primaryImageUrl ?? dto.imageUrls?.[0] ?? null,
    rating: dto.averageRating,
    reviewCount: dto.reviewCount,
  };
}

// VillageDto -> VillageCard `village` prop
export function mapVillage(dto) {
  if (!dto) return dto;
  return {
    id: dto.id,
    name: dto.name,
    craft: dto.craft,
    district: dto.districtName,
    description: dto.description,
    image: dto.imageUrl ?? null,
    districtId: dto.districtId,
  };
}
