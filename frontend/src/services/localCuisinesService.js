import apiClient from './apiClient';

export const localCuisinesService = {
  list: (params) => apiClient.get('/local-cuisines', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/local-cuisines/${id}`).then((res) => res.data),
};
