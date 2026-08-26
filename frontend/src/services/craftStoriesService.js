import apiClient from './apiClient';

export const craftStoriesService = {
  getByCategory: (categoryId) => apiClient.get(`/craft-stories/category/${categoryId}`).then((res) => res.data),
};
