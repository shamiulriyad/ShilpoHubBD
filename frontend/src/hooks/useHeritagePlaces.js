import { useQuery } from '@tanstack/react-query';
import { heritagePlacesService } from '../services/heritagePlacesService';

export function useHeritagePlaces(params = {}) {
  return useQuery({ queryKey: ['heritage-places', params], queryFn: () => heritagePlacesService.list(params) });
}

export function useNearbyHeritagePlaces(params, enabled = true) {
  return useQuery({
    queryKey: ['heritage-places', 'nearby', params],
    queryFn: () => heritagePlacesService.nearby(params),
    enabled,
  });
}

export function useHeritagePlace(id) {
  return useQuery({
    queryKey: ['heritage-places', id],
    queryFn: () => heritagePlacesService.getById(id),
    enabled: Boolean(id),
  });
}
