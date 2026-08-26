import { useQuery } from '@tanstack/react-query';
import { supplierDiscoveryService } from '../services/supplierDiscoveryService';

export function useSupplierSearch(params = {}) {
  return useQuery({ queryKey: ['supplier-discovery', 'search', params], queryFn: () => supplierDiscoveryService.search(params) });
}

export function useSupplierProfile(producerId) {
  return useQuery({
    queryKey: ['supplier-discovery', 'producer', producerId],
    queryFn: () => supplierDiscoveryService.getProfile(producerId),
    enabled: Boolean(producerId),
  });
}
