import { useQuery } from '@tanstack/react-query';
import { heritageFestivalsService } from '../services/heritageFestivalsService';

export function useHeritageFestivals(params = {}) {
  return useQuery({ queryKey: ['heritage-festivals', params], queryFn: () => heritageFestivalsService.list(params) });
}

export function useHeritageFestival(id) {
  return useQuery({
    queryKey: ['heritage-festivals', id],
    queryFn: () => heritageFestivalsService.getById(id),
    enabled: Boolean(id),
  });
}
