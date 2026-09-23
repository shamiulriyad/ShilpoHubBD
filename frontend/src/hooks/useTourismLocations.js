import { useQueries, useQuery } from '@tanstack/react-query';
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
export const ACCOMMODATION_TYPES = ['Hotel', 'Resort', 'Hostel'];

const STATUS_ORDER = { Verified: 0, SecondarySource: 1 };

// Active Hotel / Resort / Hostel records for one district, from the existing /tourism-locations
// endpoint (its `type` filter takes a single value, so this is one request per type). Nothing runs
// until a district is chosen, and the list follows the district as it changes.
export function useAccommodationLocations(districtId) {
  const queries = useQueries({
    queries: ACCOMMODATION_TYPES.map((type) => {
      const params = { type, districtId, isActive: true, pageSize: 50 };
      return {
        queryKey: ['tourism-locations', params],
        queryFn: () => tourismLocationsService.list(params),
        enabled: Boolean(districtId),
      };
    }),
  });

  const items = queries
    .flatMap((q) => q.data?.items ?? [])
    .filter((l) => l.isActive !== false && ACCOMMODATION_TYPES.includes(l.type))
    .sort(
      (a, b) =>
        (STATUS_ORDER[a.verificationStatus] ?? 2) - (STATUS_ORDER[b.verificationStatus] ?? 2) || a.name.localeCompare(b.name),
    );

  return { queries, items, isLoading: queries.some((q) => q.isLoading) };
}
