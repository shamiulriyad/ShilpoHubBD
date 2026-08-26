import { useMutation } from '@tanstack/react-query';
import { supplierMatchingService } from '../services/supplierMatchingService';

export function useSupplierMatch() {
  return useMutation({ mutationFn: (payload) => supplierMatchingService.match(payload) });
}
