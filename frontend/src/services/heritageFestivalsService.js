import apiClient from './apiClient';

export const heritageFestivalsService = {
  list: (params) => apiClient.get('/heritage-festivals', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/heritage-festivals/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/heritage-festivals', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/heritage-festivals/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/heritage-festivals/${id}`).then((res) => res.data),
};
