import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { preservationStrategiesService } from '../services/preservationStrategiesService';

export function usePreservationStrategies(params = {}) {
  return useQuery({ queryKey: ['preservation-strategies', 'list', params], queryFn: () => preservationStrategiesService.list(params) });
}

export function usePreservationStrategy(id) {
  return useQuery({
    queryKey: ['preservation-strategies', id],
    queryFn: () => preservationStrategiesService.getById(id),
    enabled: Boolean(id),
  });
}

export function usePreservationStrategyMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['preservation-strategies'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['preservation-strategies', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => preservationStrategiesService.create(payload), onSuccess: () => invalidate() }),
    update: useMutation({
      mutationFn: ({ id, payload }) => preservationStrategiesService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({ mutationFn: (id) => preservationStrategiesService.remove(id), onSuccess: () => invalidate() }),
    addObjective: useMutation({
      mutationFn: ({ id, payload }) => preservationStrategiesService.addObjective(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateObjective: useMutation({
      mutationFn: ({ id, objectiveId, payload }) => preservationStrategiesService.updateObjective(id, objectiveId, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeObjective: useMutation({
      mutationFn: ({ id, objectiveId }) => preservationStrategiesService.removeObjective(id, objectiveId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addAction: useMutation({
      mutationFn: ({ id, payload }) => preservationStrategiesService.addAction(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateAction: useMutation({
      mutationFn: ({ id, actionId, payload }) => preservationStrategiesService.updateAction(id, actionId, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeAction: useMutation({
      mutationFn: ({ id, actionId }) => preservationStrategiesService.removeAction(id, actionId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}
