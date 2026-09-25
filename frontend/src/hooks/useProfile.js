import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { profileService } from '../services/profileService';
import { useAuth } from './useAuth';

export function useMyProfile() {
  const { isAuthenticated, user } = useAuth();
  return useQuery({
    queryKey: ['profile', 'me', user?.id],
    queryFn: () => profileService.getMine(),
    enabled: isAuthenticated,
    staleTime: 30_000,
  });
}

export function useSaveProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => profileService.saveMine(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['profile'] }),
  });
}

export function useProfilePhoto() {
  const queryClient = useQueryClient();
  const refresh = () => queryClient.invalidateQueries({ queryKey: ['profile'] });
  return {
    upload: useMutation({ mutationFn: (file) => profileService.uploadPhoto(file), onSuccess: refresh }),
    remove: useMutation({ mutationFn: () => profileService.removePhoto(), onSuccess: refresh }),
  };
}

export function useProfileApprovals(params = {}) {
  return useQuery({ queryKey: ['profile', 'admin', params], queryFn: () => profileService.listForAdmin(params) });
}

export function useProfileReviewMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['profile'] });
  return {
    approve: useMutation({ mutationFn: ({ id, notes }) => profileService.approve(id, notes), onSuccess: invalidate }),
    reject: useMutation({ mutationFn: ({ id, notes }) => profileService.reject(id, notes), onSuccess: invalidate }),
  };
}

// Expertise values of approved producers (for the marketplace and supplier filters).
export function useExpertiseOptions() {
  return useQuery({ queryKey: ['profile', 'expertise-options'], queryFn: () => profileService.expertiseOptions(), staleTime: 5 * 60_000 });
}
