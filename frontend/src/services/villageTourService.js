import apiClient from './apiClient';

export const villageTourService = {
  list: (params) => apiClient.get('/village-tour-stops', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/village-tour-stops/${id}`).then((res) => res.data),
};
