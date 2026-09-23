import apiClient from './apiClient';

export const productsService = {
  uploadImage: (file) => {
    const data = new FormData();
    data.append('file', file);
    return apiClient.post('/media/images', data).then((res) => res.data);
  },
  create: payload => apiClient.post('/products', payload).then(res => res.data),
  update: (id, payload) => apiClient.put(`/products/${id}`, payload).then(res => res.data),
  remove: (id) => apiClient.delete(`/products/${id}`),
  list: (params, signal) => apiClient.get('/products', { params, signal }).then((res) => res.data),
  featured: (count = 8) => apiClient.get('/products/featured', { params: { count } }).then((res) => res.data),
  trending: (count = 8) => apiClient.get('/products/trending', { params: { count } }).then((res) => res.data),
  getById: (id) => apiClient.get(`/products/${id}`).then((res) => res.data),
  getBySlug: (slug) => apiClient.get(`/products/slug/${slug}`).then((res) => res.data),
  mine: () => apiClient.get('/products/mine').then((res) => res.data),
  setFeatured: (id, isFeatured) => apiClient.patch(`/products/${id}/featured`, { isFeatured }).then((res) => res.data),
  setHandmadeVerification: (id, payload) => apiClient.patch(`/products/${id}/handmade-verification`, payload).then((res) => res.data),
};
