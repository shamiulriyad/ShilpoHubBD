import apiClient from './apiClient';

export const villagesService = {
  list: () => apiClient.get('/villages').then((res) => res.data),
  favorites: () => apiClient.get('/villages/favorites').then((res) => res.data),
  getById: (id) => apiClient.get(`/villages/${id}`).then((res) => res.data),
  favorite: (id) => apiClient.post(`/villages/${id}/favorite`),
  unfavorite: (id) => apiClient.delete(`/villages/${id}/favorite`),
  create: (payload) => apiClient.post('/villages', payload).then((res) => res.data),
};
