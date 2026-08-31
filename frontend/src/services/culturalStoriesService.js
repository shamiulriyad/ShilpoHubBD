import apiClient from './apiClient';

export const culturalStoriesService = {
  list: (params) => apiClient.get('/cultural-stories', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/cultural-stories/${id}`).then((res) => res.data),
};
