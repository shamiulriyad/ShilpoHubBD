import apiClient from './apiClient';

export const inventoryService = {
  adjustStock: (productId, payload) => apiClient.post(`/inventory/products/${productId}/adjust`, payload).then((res) => res.data),
  history: (productId) => apiClient.get(`/inventory/products/${productId}/history`).then((res) => res.data),
  lowStock: () => apiClient.get('/inventory/low-stock').then((res) => res.data),
};
