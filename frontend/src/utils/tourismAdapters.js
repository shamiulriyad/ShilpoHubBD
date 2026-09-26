// Shared helpers for the public Tourism pages. All content comes from the API (database); nothing here is data.

export const normaliseDistrict = (value) =>
  (value || '')
    .toLowerCase()
    .replace(/[’']/g, '')
    .replace(/\bdistrict\b/g, '')
    .trim()
    .replace('barisal', 'barishal')
    .replace('chittagong', 'chattogram')
    .replace('comilla', 'cumilla')
    .replace('jessore', 'jashore')
    .replace('bogra', 'bogura')
    .replace('chapai nawabganj', 'chapainawabganj')
    .replace('maulvibazar', 'moulvibazar');

export const hasCoordinates = (place) =>
  place.latitude != null &&
  place.longitude != null &&
  Number.isFinite(Number(place.latitude)) &&
  Number.isFinite(Number(place.longitude)) &&
  Number(place.latitude) >= 20 &&
  Number(place.latitude) <= 27 &&
  Number(place.longitude) >= 88 &&
  Number(place.longitude) <= 93;

export const mapLink = (place) => `/tourism/map?place=${encodeURIComponent(place.id)}`;

const PLACE_TYPE_LABELS = { HistoricalSite: 'Historical site', CraftCenter: 'Craft centre', NaturalSite: 'Natural site' };
export const placeTypeLabel = (type) => PLACE_TYPE_LABELS[type] || (type || '').replace(/([a-z])([A-Z])/g, '$1 $2');

export const toPhoto = (url, alt, credit) => (url ? { url, alt, credit: credit || null } : null);

// HeritagePlaceDto -> the flat shape the map/cards render.
export function toPlace(dto) {
  return {
    id: dto.id,
    name: dto.name,
    districtId: dto.districtId,
    districtName: dto.districtName,
    placeType: placeTypeLabel(dto.placeType),
    description: dto.description,
    knownFor: dto.knownFor,
    latitude: dto.latitude,
    longitude: dto.longitude,
    source: dto.sourceUrl,
    image: toPhoto(dto.imageUrl, dto.name, dto.imageCredit),
    isFeatured: dto.isFeatured,
  };
}
