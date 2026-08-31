import { useQuery } from '@tanstack/react-query';
import { mentorsService } from '../services/mentorsService';

export function useMentors(params = {}) {
  return useQuery({ queryKey: ['mentors', params], queryFn: () => mentorsService.list(params) });
}

export function useMentor(id) {
  return useQuery({ queryKey: ['mentors', id], queryFn: () => mentorsService.getById(id), enabled: Boolean(id) });
}
