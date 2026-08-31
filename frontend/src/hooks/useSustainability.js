import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { sustainabilityService } from '../services/sustainabilityService';

export function useMySustainabilityProfile() {
  return useQuery({ queryKey: ['sustainability', 'me'], queryFn: () => sustainabilityService.me(), retry: false });
}

export function useProducerSustainability(producerId) {
  return useQuery({
    queryKey: ['sustainability', 'producer', producerId],
    queryFn: () => sustainabilityService.getByProducer(producerId),
    enabled: Boolean(producerId),
    retry: false,
  });
}

export function useSustainabilityMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['sustainability'] });

  const addMaterial = useMutation({ mutationFn: (payload) => sustainabilityService.addMaterial(payload), onSuccess: invalidate });
  const addCertification = useMutation({ mutationFn: (payload) => sustainabilityService.addCertification(payload), onSuccess: invalidate });
  const verifyCertification = useMutation({
    mutationFn: (certificationId) => sustainabilityService.verifyCertification(certificationId),
    onSuccess: invalidate,
  });

  return { addMaterial, addCertification, verifyCertification };
}
