import { useQuery } from '@tanstack/react-query';
import { touristServicesService } from '../services/touristServicesService';

export function useTouristServices(params = {}) {
  return useQuery({ queryKey: ['tourist-services', params], queryFn: () => touristServicesService.list(params) });
}

export function useTouristService(id) {
  return useQuery({
    queryKey: ['tourist-services', id],
    queryFn: () => touristServicesService.getById(id),
    enabled: Boolean(id),
  });
}

export function useServiceAvailabilitySlots(serviceId, params = {}) {
  return useQuery({
    queryKey: ['tourist-services', serviceId, 'slots', params],
    queryFn: () => touristServicesService.availabilitySlots(serviceId, params),
    enabled: Boolean(serviceId),
  });
}
