import { useMutation, useQuery } from '@tanstack/react-query';
import { qrVerificationService } from '../services/qrVerificationService';

export function useVerifyQRCode() {
  return useMutation({ mutationFn: (code) => qrVerificationService.verify(code) });
}

export function useMyQRHistory(params = {}) {
  return useQuery({
    queryKey: ['qr-verification', 'history', 'mine', params],
    queryFn: () => qrVerificationService.myHistory(params),
  });
}
