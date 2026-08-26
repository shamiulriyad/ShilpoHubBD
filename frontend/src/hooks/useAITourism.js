import { useMutation } from '@tanstack/react-query';
import { aiTourismService } from '../services/aiTourismService';

export function useTourPlan() {
  return useMutation({ mutationFn: (payload) => aiTourismService.tourPlan(payload) });
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
