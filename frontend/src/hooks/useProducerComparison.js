import { useMutation } from '@tanstack/react-query';
import { producerComparisonService } from '../services/producerComparisonService';

export function useProducerComparison() {
  return useMutation({ mutationFn: (producerIds) => producerComparisonService.compare(producerIds) });
}
