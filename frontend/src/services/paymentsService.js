import apiClient from './apiClient';

export const paymentsService = {
  initiate: (orderId) => apiClient.post('/payments', { orderId }).then((res) => res.data),
  listForOrder: (orderId) => apiClient.get(`/payments/order/${orderId}`).then((res) => res.data),
  getById: (id) => apiClient.get(`/payments/${id}`).then((res) => res.data),
  verify: (id) => apiClient.post(`/payments/${id}/verify`).then((res) => res.data),
};
