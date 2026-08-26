import { useQuery } from '@tanstack/react-query';
import { producerStoriesService } from '../services/producerStoriesService';

export function useProducerStory(producerId) {
  return useQuery({
    queryKey: ['producer-stories', producerId],
    queryFn: () => producerStoriesService.getByProducer(producerId),
    enabled: Boolean(producerId),
    retry: false,
  });
}
