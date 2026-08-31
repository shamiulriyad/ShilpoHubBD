import apiClient from './apiClient';

export const heritageRoutesService = {
  list: (params) => apiClient.get('/heritage-routes', { params }).then((res) => res.data),
  recommended: () => apiClient.get('/heritage-routes/recommended').then((res) => res.data),
  getById: (id) => apiClient.get(`/heritage-routes/${id}`).then((res) => res.data),
};
