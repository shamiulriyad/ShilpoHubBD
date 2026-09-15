import apiClient from './apiClient';

export const unescoRecordsService = {
  list: (params) => apiClient.get('/unesco-records', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/unesco-records/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/unesco-records', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/unesco-records/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/unesco-records/${id}`).then((res) => res.data),
};
