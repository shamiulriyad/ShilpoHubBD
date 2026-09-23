import { useHeritagePlaces } from './useHeritagePlaces';
import { destinationReferences, mapLink } from '../data/tourismGuides';
import { routePaths } from '../routes/routePaths';

// Curated, licensed reference photos already vetted for this project (see
// data/tourismReferences.json). Backend heritage places without a featured
// flag yet still render, so the gallery never looks empty pre-launch.
const fallbackItems = destinationReferences
  .filter((place) => place.image?.url)
  .map((place) => ({
    id: place.id,
    name: place.name,
    districtName: place.districtName,
    placeType: place.placeType,
    imageUrl: place.image.url,
    to: mapLink(place),
  }));

function toGalleryItem(place) {
  return {
    id: place.id,
    name: place.name,
    districtName: place.districtName,
    placeType: place.placeType,
    imageUrl: place.imageUrl,
    to: routePaths.tourismPlaceDetails.replace(':placeId', place.id),
  };
}

export function useHeritageGallery(limit = 8) {
  // Once /api/heritage-places carries featured records with photos, the
  // apiPlaces branch below takes over and the local fallback stops being used.
  const query = useHeritagePlaces({ isFeatured: true, pageSize: limit });
  const apiPlaces = Array.isArray(query.data?.items) ? query.data.items : Array.isArray(query.data) ? query.data : [];
  const items = apiPlaces.length ? apiPlaces.slice(0, limit).map(toGalleryItem) : fallbackItems.slice(0, limit);
  return { items };
}
