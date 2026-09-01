import apiClient from './apiClient';

export const shipmentsService = {
  track: (trackingNumber) => apiClient.get(`/logistics/shipments/track/${trackingNumber}`).then((res) => res.data),
  list: (params) => apiClient.get('/logistics/shipments', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/logistics/shipments/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/logistics/shipments', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/logistics/shipments/${id}`, payload).then((res) => res.data),
  updateStatus: (id, payload) => apiClient.post(`/logistics/shipments/${id}/status`, payload).then((res) => res.data),
  addEvent: (id, payload) => apiClient.post(`/logistics/shipments/${id}/events`, payload).then((res) => res.data),
  updateLocation: (id, payload) => apiClient.post(`/logistics/shipments/${id}/location`, payload).then((res) => res.data),
  recordDeliveryAttempt: (id, payload) =>
    apiClient.post(`/logistics/shipments/${id}/delivery-attempts`, payload).then((res) => res.data),
  markDelivered: (id, payload) => apiClient.post(`/logistics/shipments/${id}/deliver`, payload).then((res) => res.data),
  cancel: (id, payload) => apiClient.post(`/logistics/shipments/${id}/cancel`, payload).then((res) => res.data),
  addNote: (id, payload) => apiClient.post(`/logistics/shipments/${id}/notes`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/logistics/shipments/${id}`).then((res) => res.data),
};
