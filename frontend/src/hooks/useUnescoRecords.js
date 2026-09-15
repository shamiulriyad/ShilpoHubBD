import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { unescoRecordsService } from '../services/unescoRecordsService';

export function useUnescoRecords(params = {}) {
  return useQuery({ queryKey: ['unesco-records', params], queryFn: () => unescoRecordsService.list(params) });
}

export function useUnescoRecordMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['unesco-records'] });

  return {
    create: useMutation({ mutationFn: (payload) => unescoRecordsService.create(payload), onSuccess: invalidate }),
    update: useMutation({ mutationFn: ({ id, payload }) => unescoRecordsService.update(id, payload), onSuccess: invalidate }),
    remove: useMutation({ mutationFn: (id) => unescoRecordsService.remove(id), onSuccess: invalidate }),
  };
}
