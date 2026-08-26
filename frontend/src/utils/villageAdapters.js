export function toVillageCardItem(dto) {
  return {
    id: dto.id,
    name: dto.name,
    craft: dto.craft,
    district: dto.districtName,
  };
}
