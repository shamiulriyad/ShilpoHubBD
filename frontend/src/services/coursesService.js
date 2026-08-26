import apiClient from './apiClient';

export const coursesService = {
  list: (params) => apiClient.get('/courses', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/courses/${id}`).then((res) => res.data),
};
