import apiClient from './apiClient';

export const pickupRequestsService = {
  list: (params) => apiClient.get('/logistics/pickups', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/logistics/pickups/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/logistics/pickups', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/logistics/pickups/${id}`, payload).then((res) => res.data),
  schedule: (id, payload) => apiClient.post(`/logistics/pickups/${id}/schedule`, payload).then((res) => res.data),
  assign: (id, payload) => apiClient.post(`/logistics/pickups/${id}/assign`, payload).then((res) => res.data),
  updateStatus: (id, payload) => apiClient.post(`/logistics/pickups/${id}/status`, payload).then((res) => res.data),
  cancel: (id, payload) => apiClient.post(`/logistics/pickups/${id}/cancel`, payload).then((res) => res.data),
  addNote: (id, payload) => apiClient.post(`/logistics/pickups/${id}/notes`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/logistics/pickups/${id}`).then((res) => res.data),
};
