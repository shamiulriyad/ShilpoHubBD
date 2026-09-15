import apiClient from './apiClient';

export const adminUsersService = {
  list: (params) => apiClient.get('/admin/users', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/admin/users/${id}`).then((res) => res.data),
  activate: (id) => apiClient.post(`/admin/users/${id}/activate`).then((res) => res.data),
  deactivate: (id) => apiClient.post(`/admin/users/${id}/deactivate`).then((res) => res.data),
};
