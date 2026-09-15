import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminUsersService } from '../services/adminUsersService';

export function useAdminUsers(params = {}) {
  return useQuery({ queryKey: ['admin-users', 'list', params], queryFn: () => adminUsersService.list(params) });
}

export function useAdminUser(id) {
  return useQuery({
    queryKey: ['admin-users', 'detail', id],
    queryFn: () => adminUsersService.getById(id),
    enabled: !!id,
  });
}

export function useAdminUserMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin-users'] });

  return {
    activate: useMutation({ mutationFn: (id) => adminUsersService.activate(id), onSuccess: invalidate }),
    deactivate: useMutation({ mutationFn: (id) => adminUsersService.deactivate(id), onSuccess: invalidate }),
  };
}
