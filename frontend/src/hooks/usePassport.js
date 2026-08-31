import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { passportService } from '../services/passportService';

export function useAllBadges() {
  return useQuery({ queryKey: ['passport', 'badges', 'all'], queryFn: () => passportService.listAllBadges() });
}

export function useMyBadges() {
  return useQuery({ queryKey: ['passport', 'badges', 'mine'], queryFn: () => passportService.myBadges() });
}

export function useMyCheckIns() {
  return useQuery({ queryKey: ['passport', 'checkins', 'mine'], queryFn: () => passportService.myCheckIns() });
}

export function useMyJournal() {
  return useQuery({ queryKey: ['passport', 'journal', 'mine'], queryFn: () => passportService.myJournal() });
}

export function usePassportMutations() {
  const queryClient = useQueryClient();

  const claimDistrictBadge = useMutation({
    mutationFn: (districtId) => passportService.claimDistrictBadge(districtId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['passport', 'badges'] }),
  });

  const checkIn = useMutation({
    mutationFn: (payload) => passportService.checkIn(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['passport', 'checkins'] }),
  });

  const addJournalEntry = useMutation({
    mutationFn: (payload) => passportService.addJournalEntry(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['passport', 'journal'] }),
  });

  const deleteJournalEntry = useMutation({
    mutationFn: (id) => passportService.deleteJournalEntry(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['passport', 'journal'] }),
  });

  return { claimDistrictBadge, checkIn, addJournalEntry, deleteJournalEntry };
}
