import apiClient from './apiClient';

// Custom Orders — POST /api/custom-orders, GET mine, GET/{id}, cancel. All [Authorize].
export const customOrdersService = {
  create: (payload) => apiClient.post('/custom-orders', payload).then((res) => res.data),
  mine: () => apiClient.get('/custom-orders/mine/customer').then((res) => res.data),
  getById: (id) => apiClient.get(`/custom-orders/${id}`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/custom-orders/${id}/cancel`).then((res) => res.data),
};
