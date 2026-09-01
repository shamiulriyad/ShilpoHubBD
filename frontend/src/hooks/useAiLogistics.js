import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { aiLogisticsService } from '../services/aiLogisticsService';

export function useDeliveryPredictions(params = {}) {
  return useQuery({ queryKey: ['ai-logistics', 'predictions', params], queryFn: () => aiLogisticsService.listDeliveryPredictions(params) });
}

export function useRouteOptimizationRuns(params = {}) {
  return useQuery({ queryKey: ['ai-logistics', 'optimizations', params], queryFn: () => aiLogisticsService.listRouteOptimizations(params) });
}

export function useDemandForecasts(params = {}) {
  return useQuery({ queryKey: ['ai-logistics', 'forecasts', params], queryFn: () => aiLogisticsService.listDemandForecasts(params) });
}

export function useWarehouseAllocations(params = {}) {
  return useQuery({ queryKey: ['ai-logistics', 'allocations', params], queryFn: () => aiLogisticsService.listWarehouseAllocations(params) });
}

export function useAiLogisticsMutations() {
  const queryClient = useQueryClient();
  const invalidate = (key) => queryClient.invalidateQueries({ queryKey: ['ai-logistics', key] });

  return {
    predictDelivery: useMutation({
      mutationFn: (payload) => aiLogisticsService.predictDelivery(payload),
      onSuccess: () => invalidate('predictions'),
    }),
    optimizeRoute: useMutation({
      mutationFn: (payload) => aiLogisticsService.optimizeRoute(payload),
      onSuccess: () => invalidate('optimizations'),
    }),
    applyRouteOptimization: useMutation({
      mutationFn: (id) => aiLogisticsService.applyRouteOptimization(id),
      onSuccess: () => invalidate('optimizations'),
    }),
    forecastDemand: useMutation({
      mutationFn: (payload) => aiLogisticsService.forecastDemand(payload),
      onSuccess: () => invalidate('forecasts'),
    }),
    recommendWarehouse: useMutation({
      mutationFn: (payload) => aiLogisticsService.recommendWarehouse(payload),
      onSuccess: () => invalidate('allocations'),
    }),
  };
}
