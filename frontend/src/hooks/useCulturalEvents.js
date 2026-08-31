import { useQuery } from '@tanstack/react-query';
import { culturalEventsService } from '../services/culturalEventsService';

export function useCulturalEvents(params = {}) {
  return useQuery({ queryKey: ['cultural-events', params], queryFn: () => culturalEventsService.list(params) });
}
