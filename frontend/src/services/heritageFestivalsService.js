import apiClient from './apiClient';

export const heritageFestivalsService = {
  list: (params) => apiClient.get('/heritage-festivals', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/heritage-festivals/${id}`).then((res) => res.data),
};
