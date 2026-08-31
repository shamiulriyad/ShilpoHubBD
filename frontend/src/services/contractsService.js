import apiClient from './apiClient';

export const contractsService = {
  create: (payload) => apiClient.post('/contracts', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/contracts', { params }).then((res) => res.data),
  received: (params) => apiClient.get('/contracts/received', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/contracts/${id}`).then((res) => res.data),
  accept: (id) => apiClient.post(`/contracts/${id}/accept`).then((res) => res.data),
  reject: (id, notes) => apiClient.post(`/contracts/${id}/reject`, { notes }).then((res) => res.data),
  terminate: (id) => apiClient.post(`/contracts/${id}/terminate`).then((res) => res.data),
  renew: (id, payload) => apiClient.post(`/contracts/${id}/renew`, payload).then((res) => res.data),
  addDocument: (id, payload) => apiClient.post(`/contracts/${id}/documents`, payload).then((res) => res.data),
  updateDeliveryStatus: (id, scheduleId, payload) =>
    apiClient.post(`/contracts/${id}/delivery-schedule/${scheduleId}/status`, payload).then((res) => res.data),
};
