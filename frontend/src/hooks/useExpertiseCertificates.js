import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { expertiseCertificatesService } from '../services/expertiseCertificatesService';

export function useMyExpertise() {
  return useQuery({ queryKey: ['expertise', 'mine'], queryFn: () => expertiseCertificatesService.mine() });
}

export function useEligibleProducers() {
  return useQuery({ queryKey: ['expertise', 'eligible'], queryFn: () => expertiseCertificatesService.eligible() });
}

export function useIssueExpertiseCertificate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (producerId) => expertiseCertificatesService.issue(producerId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['expertise'] }),
  });
}
