import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { mentorshipRequestsService } from '../services/mentorshipRequestsService';

export function useMyMentorshipRequestsAsLearner() {
  return useQuery({
    queryKey: ['mentorship-requests', 'as-learner'],
    queryFn: () => mentorshipRequestsService.getMineAsLearner(),
  });
}

export function useMyMentorshipRequestsAsMentor() {
  return useQuery({
    queryKey: ['mentorship-requests', 'as-mentor'],
    queryFn: () => mentorshipRequestsService.getMineAsMentor(),
  });
}

export function useMentorshipRequestMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['mentorship-requests'] });

  return {
    create: useMutation({ mutationFn: (payload) => mentorshipRequestsService.create(payload), onSuccess: invalidate }),
    accept: useMutation({
      mutationFn: ({ id, payload }) => mentorshipRequestsService.accept(id, payload),
      onSuccess: invalidate,
    }),
    reject: useMutation({
      mutationFn: ({ id, payload }) => mentorshipRequestsService.reject(id, payload),
      onSuccess: invalidate,
    }),
    complete: useMutation({ mutationFn: (id) => mentorshipRequestsService.complete(id), onSuccess: invalidate }),
  };
}
