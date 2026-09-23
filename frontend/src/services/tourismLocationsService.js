import apiClient from './apiClient';

export const tourismLocationsService = {
  list: (params) => apiClient.get('/tourism-locations', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/tourism-locations/${id}`).then((res) => res.data),
};
