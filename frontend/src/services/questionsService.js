import apiClient from './apiClient';

export const questionsService = {
  listForProduct: (productId, params) =>
    apiClient.get(`/questions/product/${productId}`, { params }).then((res) => res.data),
  listMine: (params) => apiClient.get('/questions/mine', { params }).then((res) => res.data),
  ask: (productId, body, imageUrl) => apiClient.post(`/questions/product/${productId}`, { body, imageUrl: imageUrl || undefined }).then((res) => res.data),
  answer: (id, body) => apiClient.post(`/questions/${id}/answers`, { body }).then((res) => res.data),
};
