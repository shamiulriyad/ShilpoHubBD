import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { heritageDatabaseService } from '../services/heritageDatabaseService';

export function useHeritageDbSummary(params = {}) {
  return useQuery({ queryKey: ['heritage-db', 'summary', params], queryFn: () => heritageDatabaseService.getSummary(params) });
}

export function useHeritageDatasets(params = {}) {
  return useQuery({ queryKey: ['heritage-db', 'datasets', params], queryFn: () => heritageDatabaseService.listDatasets(params) });
}

export function useHeritageRiskRecords(params = {}) {
  return useQuery({ queryKey: ['heritage-db', 'risk', params], queryFn: () => heritageDatabaseService.listRiskRecords(params) });
}

export function useHeritageDatabaseMutations() {
  const queryClient = useQueryClient();
  return {
    createDataset: useMutation({
      mutationFn: (payload) => heritageDatabaseService.createDataset(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['heritage-db', 'datasets'] }),
    }),
    refreshDataset: useMutation({
      mutationFn: (id) => heritageDatabaseService.refreshDataset(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['heritage-db', 'datasets'] }),
    }),
    removeDataset: useMutation({
      mutationFn: (id) => heritageDatabaseService.removeDataset(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['heritage-db', 'datasets'] }),
    }),
    createRiskRecord: useMutation({
      mutationFn: (payload) => heritageDatabaseService.createRiskRecord(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['heritage-db', 'risk'] }),
    }),
    removeRiskRecord: useMutation({
      mutationFn: (id) => heritageDatabaseService.removeRiskRecord(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['heritage-db', 'risk'] }),
    }),
  };
}
