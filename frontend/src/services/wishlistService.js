import apiClient from './apiClient';

export const wishlistService = {
  list: () => apiClient.get('/wishlist').then((res) => res.data),
  add: (productId) => apiClient.post('/wishlist', { productId }).then((res) => res.data),
  remove: (productId) => apiClient.delete(`/wishlist/${productId}`),
  moveToCart: (productId, payload = {}) =>
    apiClient.post(`/wishlist/${productId}/move-to-cart`, payload).then((res) => res.data),
};
