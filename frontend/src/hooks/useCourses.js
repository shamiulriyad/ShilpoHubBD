import { useQuery } from '@tanstack/react-query';
import { coursesService } from '../services/coursesService';

export function useCourses(params = {}) {
  return useQuery({ queryKey: ['courses', params], queryFn: () => coursesService.list(params) });
}

export function useCourse(id) {
  return useQuery({ queryKey: ['courses', id], queryFn: () => coursesService.getById(id), enabled: Boolean(id) });
}
