import apiClient from './apiClient';

export const heritagePlacesService = {
  list: (params) => apiClient.get('/heritage-places', { params }).then((res) => res.data),
  nearby: (params) => apiClient.get('/heritage-places/nearby', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/heritage-places/${id}`).then((res) => res.data),
};
