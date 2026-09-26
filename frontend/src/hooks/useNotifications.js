import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/apiClient';
import { useAuth } from './useAuth';
export function useNotifications({ unreadOnly = false, page = 1 } = {}) {
  const { user, isAuthenticated } = useAuth();
  return useQuery({ queryKey: ['notifications', user?.id, { unreadOnly, page }],
    queryFn: ({ signal }) => apiClient.get('/notifications', { params: { unreadOnly, page }, signal }).then(r => r.data),
    enabled: isAuthenticated, staleTime: 15000, refetchInterval: 30000, refetchIntervalInBackground: false });
}
export function useNotificationActions() {
  const client = useQueryClient();
  return useMutation({ mutationFn: action => action.all
    ? apiClient.post('/notifications/read-all', { through: action.through })
    : apiClient.patch(`/notifications/${action.id}/read`, { isRead: action.isRead }),
    onSuccess: () => client.invalidateQueries({ queryKey: ['notifications'] }), meta: { silent: true } });
}
