import apiClient from './apiClient';

export const mentorsService = {
  list: (params) => apiClient.get('/mentors', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/mentors/${id}`).then((res) => res.data),
};
