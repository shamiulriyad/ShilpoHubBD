import apiClient from './apiClient';

export const rolesService = {
  assign: (userId, role) => apiClient.post('/roles/assign', { userId, role }).then((res) => res.data),
  remove: (userId, role) => apiClient.post('/roles/remove', { userId, role }).then((res) => res.data),
};
