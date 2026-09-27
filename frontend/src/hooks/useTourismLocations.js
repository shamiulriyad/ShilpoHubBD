import { useQuery } from '@tanstack/react-query';
import { tourismLocationsService } from '../services/tourismLocationsService';

export function useTourismLocations(params = {}) {
  return useQuery({ queryKey: ['tourism-locations', params], queryFn: () => tourismLocationsService.list(params) });
}

export function useTourismLocation(id) {
  return useQuery({
    queryKey: ['tourism-locations', id],
    queryFn: () => tourismLocationsService.getById(id),
    enabled: Boolean(id),
  });
}

// The TourismLocation types that count as somewhere to stay.
export const ACCOMMODATION_TYPES = ['Hotel', 'Resort', 'Hostel', 'GuestHouse', 'Motel', 'Homestay'];

// Where to stay for one district: admin-entered records plus OpenStreetMap listings, from one
// backend call (the backend refreshes stale OpenStreetMap data itself and falls back to what it has
// stored if that source is down). Nothing runs until a district is chosen.
export function useAccommodationLocations(districtId) {
  const query = useQuery({
    queryKey: ['tourism-accommodations', districtId],
    queryFn: () => tourismLocationsService.accommodations(districtId),
    enabled: Boolean(districtId),
    staleTime: 5 * 60 * 1000,
  });
  return { queries: [query], items: query.data ?? [], isLoading: query.isLoading };
}

// Accommodation around the destination's coordinates, nearest first. The backend pulls it from
// OpenStreetMap on demand; while that import is still running it says so and we ask again a few
// times instead of showing an empty list.
export function useNearbyAccommodations(districtId, radiusKm) {
  return useQuery({
    queryKey: ['tourism-accommodations-nearby', districtId, radiusKm],
    queryFn: () => tourismLocationsService.nearbyAccommodations(districtId, radiusKm),
    enabled: Boolean(districtId),
    staleTime: 5 * 60 * 1000,
    refetchInterval: (query) => (query.state.data?.isImporting && query.state.dataUpdateCount < 8 ? 8000 : false),
  });
}

// Restaurants, attractions, heritage, museums, parks... for one district (same source rules).
export function useTourismPois(districtId) {
  const query = useQuery({
    queryKey: ['tourism-pois', districtId],
    queryFn: () => tourismLocationsService.pois(districtId),
    enabled: Boolean(districtId),
    staleTime: 5 * 60 * 1000,
    // An empty first answer usually means the OpenStreetMap import is still running in the background.
    refetchInterval: (query) => (query.state.data?.length === 0 && query.state.dataUpdateCount < 4 ? 10000 : false),
  });
  return { query, items: query.data ?? [] };
}

// Card grouping for the "More places" section.
export const POI_GROUPS = [
  ['Restaurants & cafes', ['Restaurant', 'Cafe']],
  ['Attractions', ['Attraction', 'TouristPlace', 'Viewpoint', 'Beach']],
  ['Heritage & historical', ['HeritageSite', 'HistoricalPlace', 'Mosque', 'Temple']],
  ['Museums', ['Museum']],
  ['Parks', ['Park']],
];
