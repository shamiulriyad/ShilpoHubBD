import { useQuery, useMutation } from '@tanstack/react-query';
import { trainingCertificatesService } from '../services/trainingCertificatesService';

export function useMyTrainingCertificates() {
  return useQuery({ queryKey: ['training-certificates', 'mine'], queryFn: () => trainingCertificatesService.mine() });
}

export function useVerifyTrainingCertificate() {
  return useMutation({ mutationFn: (certificateNumber) => trainingCertificatesService.verify(certificateNumber) });
}
