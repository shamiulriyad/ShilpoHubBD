import { useQuery } from '@tanstack/react-query';
import { culturalStoriesService } from '../services/culturalStoriesService';

export function useCulturalStories(params = {}) {
  return useQuery({ queryKey: ['cultural-stories', params], queryFn: () => culturalStoriesService.list(params) });
}
