import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bookingsService } from '../services/bookingsService';

export function useMyBookings(params = {}) {
  return useQuery({ queryKey: ['bookings', 'mine', params], queryFn: () => bookingsService.mine(params) });
}

export function useBooking(id) {
  return useQuery({ queryKey: ['bookings', id], queryFn: () => bookingsService.getById(id), enabled: Boolean(id) });
}

export function useBookingMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['bookings'] });

  const create = useMutation({ mutationFn: (payload) => bookingsService.create(payload), onSuccess: invalidate });
  const cancel = useMutation({
    mutationFn: ({ id, reason }) => bookingsService.cancel(id, reason ? { reason } : {}),
    onSuccess: invalidate,
  });

  return { create, cancel };
}
