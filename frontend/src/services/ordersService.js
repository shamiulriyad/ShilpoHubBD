import apiClient from './apiClient';

export const ordersService = {
  list: (params) => apiClient.get('/orders', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/orders/${id}`).then((res) => res.data),
  getTracking: (id) => apiClient.get(`/orders/${id}/tracking`).then((res) => res.data),
  checkout: (payload) => apiClient.post('/orders/checkout', payload).then((res) => res.data),
  cancel: (id, payload = {}) => apiClient.post(`/orders/${id}/cancel`, payload).then((res) => res.data),
  requestReturn: (id, payload) => apiClient.post(`/orders/${id}/return`, payload).then((res) => res.data),
};
