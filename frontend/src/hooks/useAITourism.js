import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { aiTourismService } from '../services/aiTourismService';

const SAVED_PLANS_KEY = ['saved-tour-plans'];

export function useTourPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => aiTourismService.tourPlan(payload),
    // A generated plan is stored server-side; refresh the "My Trip Plans" list.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: SAVED_PLANS_KEY }),
  });
}

export function useSavedTourPlans(params = {}) {
  return useQuery({ queryKey: [...SAVED_PLANS_KEY, params], queryFn: () => aiTourismService.savedPlans(params) });
}

export function useSavedTourPlan(id) {
  return useQuery({
    queryKey: [...SAVED_PLANS_KEY, id],
    queryFn: () => aiTourismService.savedPlan(id),
    enabled: Boolean(id),
  });
}

export function useDeleteSavedTourPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id) => aiTourismService.deleteSavedPlan(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: SAVED_PLANS_KEY }),
  });
}

export function useBudgetPlan() {
  return useMutation({ mutationFn: (payload) => aiTourismService.budgetPlan(payload) });
}

export function useRouteOptimization() {
  return useMutation({ mutationFn: (payload) => aiTourismService.routeOptimization(payload) });
}

export function useCulturalRecommendationsAI() {
  return useMutation({ mutationFn: (payload) => aiTourismService.culturalRecommendations(payload) });
}
