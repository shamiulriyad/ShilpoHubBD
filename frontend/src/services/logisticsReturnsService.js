import apiClient from './apiClient';

export const logisticsReturnsService = {
  list: (params) => apiClient.get('/logistics/returns', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/logistics/returns/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/logistics/returns', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/logistics/returns/${id}`, payload).then((res) => res.data),
  approve: (id, payload) => apiClient.post(`/logistics/returns/${id}/approve`, payload).then((res) => res.data),
  reject: (id, payload) => apiClient.post(`/logistics/returns/${id}/reject`, payload).then((res) => res.data),
  schedulePickup: (id, payload) => apiClient.post(`/logistics/returns/${id}/schedule-pickup`, payload).then((res) => res.data),
  updateStatus: (id, payload) => apiClient.post(`/logistics/returns/${id}/status`, payload).then((res) => res.data),
  recordInspection: (id, payload) => apiClient.post(`/logistics/returns/${id}/inspections`, payload).then((res) => res.data),
  restock: (id, payload) => apiClient.post(`/logistics/returns/${id}/restock`, payload).then((res) => res.data),
  recordRefund: (id, payload) => apiClient.post(`/logistics/returns/${id}/refund`, payload).then((res) => res.data),
  close: (id, payload) => apiClient.post(`/logistics/returns/${id}/close`, payload).then((res) => res.data),
  cancel: (id, payload) => apiClient.post(`/logistics/returns/${id}/cancel`, payload).then((res) => res.data),
  addNote: (id, payload) => apiClient.post(`/logistics/returns/${id}/notes`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/logistics/returns/${id}`).then((res) => res.data),
};
