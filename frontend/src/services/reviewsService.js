import apiClient from './apiClient';

export const reviewsService = {
  listForProduct: (productId, params) =>
    apiClient.get(`/reviews/product/${productId}`, { params }).then((res) => res.data),
  listForHeritagePlace: (heritagePlaceId, params) =>
    apiClient.get(`/reviews/heritage-place/${heritagePlaceId}`, { params }).then((res) => res.data),
  listForTouristService: (touristServiceId, params) =>
    apiClient.get(`/reviews/service/${touristServiceId}`, { params }).then((res) => res.data),
  create: (payload) => apiClient.post('/reviews', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/reviews/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/reviews/${id}`),
};
