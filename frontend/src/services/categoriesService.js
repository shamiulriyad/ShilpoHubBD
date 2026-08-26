import apiClient from './apiClient';

export const categoriesService = {
  list: () => apiClient.get('/categories').then((res) => res.data),
  getById: (id) => apiClient.get(`/categories/${id}`).then((res) => res.data),
};
