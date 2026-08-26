import apiClient from './apiClient';

export const recommendationsService = {
  forMe: (count = 8) => apiClient.get('/recommendations', { params: { count } }).then((res) => res.data),
  similarTo: (productId, count = 8) =>
    apiClient.get(`/recommendations/similar/${productId}`, { params: { count } }).then((res) => res.data),
};
