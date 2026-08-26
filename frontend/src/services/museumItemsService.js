import apiClient from './apiClient';

export const museumItemsService = {
  list: (params) => apiClient.get('/museum-items', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/museum-items/${id}`).then((res) => res.data),
};
