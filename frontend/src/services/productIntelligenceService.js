import apiClient from './apiClient';

export const productIntelligenceService = {
  get: (productId, range) => apiClient.get(`/product-intelligence/${productId}`, { params: { range } }).then((res) => res.data),
  getAiInsights: (productId, range) => apiClient.post(`/product-intelligence/${productId}/ai-insights`, null, { params: { range } }).then((res) => res.data),
};
