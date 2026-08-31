import { useQuery } from '@tanstack/react-query';
import { districtsService } from '../services/districtsService';

export function useDistricts() {
  return useQuery({
    queryKey: ['districts'],
    queryFn: () => districtsService.list(),
  });
}
