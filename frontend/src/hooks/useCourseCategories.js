import { useQuery } from '@tanstack/react-query';
import { courseCategoriesService } from '../services/courseCategoriesService';

export function useCourseCategories() {
  return useQuery({ queryKey: ['course-categories'], queryFn: () => courseCategoriesService.list() });
}
