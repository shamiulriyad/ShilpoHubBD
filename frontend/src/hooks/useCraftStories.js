import { useQuery } from '@tanstack/react-query';
import { craftStoriesService } from '../services/craftStoriesService';

export function useCraftStory(categoryId) {
  return useQuery({
    queryKey: ['craft-stories', categoryId],
    queryFn: () => craftStoriesService.getByCategory(categoryId),
    enabled: Boolean(categoryId),
    retry: false,
  });
}
