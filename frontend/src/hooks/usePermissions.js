import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { permissionsService } from '../services/permissionsService';

export function usePermissionsList() {
  return useQuery({ queryKey: ['permissions', 'list'], queryFn: () => permissionsService.list() });
}

export function useAdminRoles() {
  return useQuery({ queryKey: ['permissions', 'roles'], queryFn: () => permissionsService.listRoles() });
}

export function useRolePermissions(roleId) {
  return useQuery({
    queryKey: ['permissions', 'role', roleId],
    queryFn: () => permissionsService.getRolePermissions(roleId),
    enabled: !!roleId,
  });
}

export function usePermissionMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['permissions'] });

  return {
    createPermission: useMutation({ mutationFn: (payload) => permissionsService.create(payload), onSuccess: invalidate }),
    removePermission: useMutation({ mutationFn: (id) => permissionsService.remove(id), onSuccess: invalidate }),
    syncRolePermissions: useMutation({
      mutationFn: ({ roleId, permissionCodes }) => permissionsService.syncRolePermissions(roleId, permissionCodes),
      onSuccess: invalidate,
    }),
  };
}
