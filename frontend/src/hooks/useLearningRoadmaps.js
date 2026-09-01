import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { learningRoadmapsService } from '../services/learningRoadmapsService';

export function useActiveRoadmap() {
  return useQuery({
    queryKey: ['learning-roadmaps', 'active'],
    queryFn: () => learningRoadmapsService.getActive(),
    retry: false,
  });
}

export function useRoadmapHistory() {
  return useQuery({
    queryKey: ['learning-roadmaps', 'history'],
    queryFn: () => learningRoadmapsService.getHistory(),
  });
}

export function useLearningRoadmapMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['learning-roadmaps'] });

  return {
    create: useMutation({ mutationFn: (payload) => learningRoadmapsService.create(payload), onSuccess: invalidate }),
    refreshProgress: useMutation({ mutationFn: (id) => learningRoadmapsService.refreshProgress(id), onSuccess: invalidate }),
    completeMilestone: useMutation({
      mutationFn: ({ id, milestoneId }) => learningRoadmapsService.completeMilestone(id, milestoneId),
      onSuccess: invalidate,
    }),
  };
}
