import { useQuery } from '@tanstack/react-query';
import { supplierDiscoveryService } from '../services/supplierDiscoveryService';
import { mergeRolePage } from '../data/roleDemoData';

export function useSupplierSearch(params = {}) {
  return useQuery({ queryKey: ['supplier-discovery', 'search', params], queryFn: async () => mergeRolePage(await supplierDiscoveryService.search(params), 'suppliers') });
}

export function useSupplierProfile(producerId) {
  return useQuery({
    queryKey: ['supplier-discovery', 'producer', producerId],
    queryFn: () => supplierDiscoveryService.getProfile(producerId),
    enabled: Boolean(producerId),
  });
}

export function useSupplierBusinessProfile(producerId) {
  return useQuery({
    queryKey: ['supplier-discovery', 'producer', producerId, 'business-profile'],
    queryFn: () => supplierDiscoveryService.getBusinessProfile(producerId),
    enabled: Boolean(producerId),
  });
}
