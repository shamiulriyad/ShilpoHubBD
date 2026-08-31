import { useQuery } from '@tanstack/react-query';
import { localCuisinesService } from '../services/localCuisinesService';

export function useLocalCuisines(params = {}) {
  return useQuery({ queryKey: ['local-cuisines', params], queryFn: () => localCuisinesService.list(params) });
}
