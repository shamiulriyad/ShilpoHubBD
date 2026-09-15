import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { identityVerificationService } from '../services/identityVerificationService';

export function useMyIdentityVerifications() {
  return useQuery({ queryKey: ['identity-verifications', 'mine'], queryFn: () => identityVerificationService.mine() });
}

export function useIdentityVerifications(params = {}) {
  return useQuery({
    queryKey: ['identity-verifications', 'list', params],
    queryFn: () => identityVerificationService.list(params),
  });
}

export function useIdentityVerificationMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['identity-verifications'] });

  return {
    submit: useMutation({ mutationFn: (payload) => identityVerificationService.submit(payload), onSuccess: invalidate }),
    approve: useMutation({ mutationFn: (id) => identityVerificationService.approve(id), onSuccess: invalidate }),
    reject: useMutation({
      mutationFn: ({ id, rejectionReason }) => identityVerificationService.reject(id, rejectionReason),
      onSuccess: invalidate,
    }),
  };
}
