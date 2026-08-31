// Adapts backend Product DTOs (ProductListItemDto / ProductDto) to the flat shape
// ProductCard/EntityCard already expect, so those shared components don't need to change.
export function toProductCardItem(dto) {
  return {
    id: dto.id,
    name: dto.name,
    price: dto.discountPrice ?? dto.price,
    category: dto.categoryName,
    producer: dto.producerName,
    producerId: dto.producerId,
    district: dto.districtName,
    image: dto.primaryImageUrl ?? null,
  };
}

export function toCategoryCardItem(dto) {
  return {
    id: dto.id,
    name: dto.name,
    itemCount: dto.productCount,
  };
}
