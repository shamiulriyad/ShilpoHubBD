import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { expertiseCertificatesService } from '../services/expertiseCertificatesService';
import { producerDemoData } from '../data/producerDemoData';

export function useMyExpertise() {
  return useQuery({ queryKey: ['expertise', 'mine'], queryFn: async () => {
    const live = await expertiseCertificatesService.mine();
    return import.meta.env.DEV && !live?.ratingCount ? producerDemoData.expertise : live;
  } });
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
