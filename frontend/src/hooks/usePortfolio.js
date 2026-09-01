import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { portfolioService } from '../services/portfolioService';

export function useMyPortfolio() {
  return useQuery({
    queryKey: ['portfolio', 'me'],
    queryFn: () => portfolioService.getMine(),
    retry: false,
  });
}

export function usePublicPortfolio(academyMemberProfileId) {
  return useQuery({
    queryKey: ['portfolio', academyMemberProfileId],
    queryFn: () => portfolioService.getPublic(academyMemberProfileId),
    enabled: Boolean(academyMemberProfileId),
  });
}

export function usePortfolioMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['portfolio', 'me'] });

  return {
    updateMine: useMutation({ mutationFn: (payload) => portfolioService.updateMine(payload), onSuccess: invalidate }),
    updateVisibility: useMutation({ mutationFn: (payload) => portfolioService.updateVisibility(payload), onSuccess: invalidate }),
    addProject: useMutation({ mutationFn: (payload) => portfolioService.addProject(payload), onSuccess: invalidate }),
    updateProject: useMutation({
      mutationFn: ({ projectId, payload }) => portfolioService.updateProject(projectId, payload),
      onSuccess: invalidate,
    }),
    removeProject: useMutation({ mutationFn: (projectId) => portfolioService.removeProject(projectId), onSuccess: invalidate }),
  };
}
