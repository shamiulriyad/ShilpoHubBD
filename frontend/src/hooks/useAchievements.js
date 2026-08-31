import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { achievementsService } from '../services/achievementsService';

export function useMyXpSummary() {
  return useQuery({ queryKey: ['achievements', 'xp', 'mine'], queryFn: () => achievementsService.myXpSummary() });
}

export function useAllAchievements() {
  return useQuery({ queryKey: ['achievements', 'all'], queryFn: () => achievementsService.listAll() });
}

export function useMyAchievements() {
  return useQuery({ queryKey: ['achievements', 'mine'], queryFn: () => achievementsService.mine() });
}

export function useEvaluateAchievements() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => achievementsService.evaluate(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['achievements'] });
    },
  });
}
