import apiClient from './apiClient';

export const warehouseStockService = {
  listItems: (params) => apiClient.get('/logistics/warehouse-stock', { params }).then((res) => res.data),
  listMovements: (params) => apiClient.get('/logistics/warehouse-stock/movements', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/logistics/warehouse-stock/${id}`).then((res) => res.data),
  receive: (payload) => apiClient.post('/logistics/warehouse-stock/receive', payload).then((res) => res.data),
  issue: (id, payload) => apiClient.post(`/logistics/warehouse-stock/${id}/issue`, payload).then((res) => res.data),
  transfer: (id, payload) => apiClient.post(`/logistics/warehouse-stock/${id}/transfer`, payload).then((res) => res.data),
  adjust: (id, payload) => apiClient.post(`/logistics/warehouse-stock/${id}/adjust`, payload).then((res) => res.data),
  reserve: (id, payload) => apiClient.post(`/logistics/warehouse-stock/${id}/reserve`, payload).then((res) => res.data),
  release: (id, payload) => apiClient.post(`/logistics/warehouse-stock/${id}/release`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/logistics/warehouse-stock/${id}`).then((res) => res.data),
};
