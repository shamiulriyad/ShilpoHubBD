import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { mentorFeedbackService } from '../services/mentorFeedbackService';

export function useMyMentorFeedback() {
  return useQuery({ queryKey: ['mentor-feedback', 'mine'], queryFn: () => mentorFeedbackService.getMine() });
}

export function useSubmitMentorFeedback() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => mentorFeedbackService.submit(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['mentor-feedback'] }),
  });
}
