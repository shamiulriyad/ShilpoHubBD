import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { innovationPrototypesService } from '../services/innovationPrototypesService';

export function useInnovationPrototypes(params = {}) {
  return useQuery({ queryKey: ['innovation-prototypes', 'list', params], queryFn: () => innovationPrototypesService.list(params) });
}

export function useInnovationPrototype(id) {
  return useQuery({
    queryKey: ['innovation-prototypes', id],
    queryFn: () => innovationPrototypesService.getById(id),
    enabled: Boolean(id),
  });
}

export function usePrototypeIssues(id) {
  return useQuery({
    queryKey: ['innovation-prototypes', id, 'issues'],
    queryFn: () => innovationPrototypesService.listIssues(id),
    enabled: Boolean(id),
  });
}

export function useInnovationPrototypeMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['innovation-prototypes'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['innovation-prototypes', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => innovationPrototypesService.create(payload), onSuccess: () => invalidate() }),
    update: useMutation({
      mutationFn: ({ id, payload }) => innovationPrototypesService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({ mutationFn: (id) => innovationPrototypesService.remove(id), onSuccess: () => invalidate() }),
    addIteration: useMutation({
      mutationFn: ({ id, payload }) => innovationPrototypesService.addIteration(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addTestCase: useMutation({
      mutationFn: ({ id, payload }) => innovationPrototypesService.addTestCase(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeTestCase: useMutation({
      mutationFn: ({ id, testCaseId }) => innovationPrototypesService.removeTestCase(id, testCaseId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addIssue: useMutation({
      mutationFn: ({ id, payload }) => innovationPrototypesService.addIssue(id, payload),
      onSuccess: (_, { id }) => queryClient.invalidateQueries({ queryKey: ['innovation-prototypes', id, 'issues'] }),
    }),
    updateIssue: useMutation({
      mutationFn: ({ id, issueId, payload }) => innovationPrototypesService.updateIssue(id, issueId, payload),
      onSuccess: (_, { id }) => queryClient.invalidateQueries({ queryKey: ['innovation-prototypes', id, 'issues'] }),
    }),
  };
}
