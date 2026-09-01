import { useMutation } from '@tanstack/react-query';
import { mentorMatchingService } from '../services/mentorMatchingService';

export function useMentorMatch() {
  return useMutation({ mutationFn: (payload) => mentorMatchingService.match(payload) });
}
