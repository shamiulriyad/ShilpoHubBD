import { useHeritagePlaces } from './useHeritagePlaces';
import { routePaths } from '../routes/routePaths';

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

// Published heritage places that have a photo; featured ones first. Content is managed by admins.
export function useHeritageGallery(limit = 8) {
  const query = useHeritagePlaces({ pageSize: 50 });
  const places = Array.isArray(query.data?.items) ? query.data.items : [];
  const items = places
    .filter((place) => place.imageUrl)
    .sort((a, b) => Number(b.isFeatured) - Number(a.isFeatured))
    .slice(0, limit)
    .map(toGalleryItem);
  return { items };
}
