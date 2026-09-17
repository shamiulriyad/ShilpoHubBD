// Adapts backend Product DTOs (ProductListItemDto / ProductDto) to the flat shape
// ProductCard/EntityCard already expect, so those shared components don't need to change.
// Some legacy clay-pot records were assigned the Jamdani seed category.
// Correct that specific display mismatch; leave other catalog categories intact.
function displayCategory(dto) {
  const isClayPot = /^clay\s+pot\b/i.test((dto.name || '').trim());
  const isLegacyCategory = /^jamdani(?:\s+weaving)?$/i.test((dto.categoryName || '').trim());
  return isClayPot && isLegacyCategory ? 'Pottery' : dto.categoryName;
}

export function toProductCardItem(dto) {
  return {
    id: dto.id,
    name: dto.name,
    price: dto.discountPrice ?? dto.price,
    category: displayCategory(dto),
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
