import apiClient from './apiClient';

export const courseCategoriesService = {
  list: (activeOnly = true) => apiClient.get('/course-categories', { params: { activeOnly } }).then((res) => res.data),
};
