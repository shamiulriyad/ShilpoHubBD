import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { innovationExperimentsService } from '../services/innovationExperimentsService';

export function useInnovationExperiments(params = {}) {
  return useQuery({ queryKey: ['innovation-experiments', 'list', params], queryFn: () => innovationExperimentsService.list(params) });
}

export function useInnovationExperiment(id) {
  return useQuery({
    queryKey: ['innovation-experiments', id],
    queryFn: () => innovationExperimentsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useInnovationExperimentMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['innovation-experiments'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['innovation-experiments', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => innovationExperimentsService.create(payload), onSuccess: () => invalidate() }),
    update: useMutation({
      mutationFn: ({ id, payload }) => innovationExperimentsService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({ mutationFn: (id) => innovationExperimentsService.remove(id), onSuccess: () => invalidate() }),
    addVersion: useMutation({
      mutationFn: ({ id, payload }) => innovationExperimentsService.addVersion(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    createRun: useMutation({
      mutationFn: ({ id, payload }) => innovationExperimentsService.createRun(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateRun: useMutation({
      mutationFn: ({ id, runId, payload }) => innovationExperimentsService.updateRun(id, runId, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeRun: useMutation({
      mutationFn: ({ id, runId }) => innovationExperimentsService.removeRun(id, runId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}
