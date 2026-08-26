import { useQuery } from '@tanstack/react-query';
import { impactService } from '../services/impactService';

export function useMyImpact() {
  return useQuery({ queryKey: ['impact', 'mine'], queryFn: () => impactService.mine() });
}
