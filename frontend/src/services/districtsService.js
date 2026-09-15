import apiClient from './apiClient';

export const districtsService = {
  list: (params) => apiClient.get('/districts', { params }).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/districts/${id}`, payload).then((res) => res.data),
};
