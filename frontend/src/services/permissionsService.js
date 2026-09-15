import apiClient from './apiClient';

export const permissionsService = {
  list: () => apiClient.get('/admin/permissions').then((res) => res.data),
  create: (payload) => apiClient.post('/admin/permissions', payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/admin/permissions/${id}`).then((res) => res.data),

  listRoles: () => apiClient.get('/admin/roles').then((res) => res.data),
  getRolePermissions: (roleId) => apiClient.get(`/admin/roles/${roleId}/permissions`).then((res) => res.data),
  syncRolePermissions: (roleId, permissionCodes) =>
    apiClient.put(`/admin/roles/${roleId}/permissions`, { permissionCodes }).then((res) => res.data),
};
