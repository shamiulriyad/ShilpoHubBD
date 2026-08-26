import apiClient from './apiClient';

export const producerOrdersService = {
  list: (params) => apiClient.get('/producer/orders', { params }).then((res) => res.data),
  getById: (orderItemId) => apiClient.get(`/producer/orders/${orderItemId}`).then((res) => res.data),
  accept: (orderItemId) => apiClient.post(`/producer/orders/${orderItemId}/accept`).then((res) => res.data),
  reject: (orderItemId, reason) => apiClient.post(`/producer/orders/${orderItemId}/reject`, { reason }).then((res) => res.data),
  startProcessing: (orderItemId) => apiClient.post(`/producer/orders/${orderItemId}/processing`).then((res) => res.data),
  ship: (orderItemId, payload) => apiClient.post(`/producer/orders/${orderItemId}/ship`, payload).then((res) => res.data),
  deliver: (orderItemId) => apiClient.post(`/producer/orders/${orderItemId}/deliver`).then((res) => res.data),
  customers: () => apiClient.get('/producer/orders/customers').then((res) => res.data),
  revenue: (params) => apiClient.get('/producer/orders/analytics/revenue', { params }).then((res) => res.data),
  sales: (params) => apiClient.get('/producer/orders/analytics/sales', { params }).then((res) => res.data),
  visitors: () => apiClient.get('/producer/orders/analytics/visitors').then((res) => res.data),
  incomeReport: (params) => apiClient.get('/producer/orders/analytics/income-report', { params }).then((res) => res.data),
  productPerformance: () => apiClient.get('/producer/orders/analytics/product-performance').then((res) => res.data),
};
