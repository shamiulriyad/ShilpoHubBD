import apiClient from './apiClient';

export const cartService = {
  list: () => apiClient.get('/cart').then((res) => res.data),
  summary: () => apiClient.get('/cart/summary').then((res) => res.data),
  add: (payload) => apiClient.post('/cart', payload).then((res) => res.data),
  updateQuantity: (itemId, quantity) =>
    apiClient.put(`/cart/${itemId}`, { quantity }).then((res) => res.data),
  remove: (itemId) => apiClient.delete(`/cart/${itemId}`),
  clear: () => apiClient.delete('/cart'),
};
