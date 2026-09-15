<<<<<<< HEAD
import apiClient from './apiClient';

export const categoriesService = {
  list: () => apiClient.get('/categories').then((res) => res.data),
  getById: (id) => apiClient.get(`/categories/${id}`).then((res) => res.data),
};
=======
import apiClient from './apiClient';

export const categoriesService = {
  list: () => apiClient.get('/categories').then((res) => res.data),
  getById: (id) => apiClient.get(`/categories/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/categories', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/categories/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/categories/${id}`).then((res) => res.data),
};
>>>>>>> Riyad
