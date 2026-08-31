import { useQuery } from '@tanstack/react-query';
import { heritageRoutesService } from '../services/heritageRoutesService';

export function useHeritageRoutes(params = {}) {
  return useQuery({ queryKey: ['heritage-routes', params], queryFn: () => heritageRoutesService.list(params) });
}

export function useRecommendedHeritageRoutes() {
  return useQuery({ queryKey: ['heritage-routes', 'recommended'], queryFn: () => heritageRoutesService.recommended() });
}

export function useHeritageRoute(id) {
  return useQuery({
    queryKey: ['heritage-routes', id],
    queryFn: () => heritageRoutesService.getById(id),
    enabled: Boolean(id),
  });
}
