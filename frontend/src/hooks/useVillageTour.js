import { useQuery } from '@tanstack/react-query';
import { villageTourService } from '../services/villageTourService';

export function useVillageTourStops(params = {}) {
  return useQuery({ queryKey: ['village-tour-stops', params], queryFn: () => villageTourService.list(params) });
}
