import apiClient from './apiClient';

// Producer-confirmed search attributes for a product, plus AI suggestions (which stay pending until the producer confirms).
export const productAttributesService = {
  get: (productId) => apiClient.get(`/products/${productId}/attributes`).then((res) => res.data),
  save: (productId, payload) => apiClient.put(`/products/${productId}/attributes`, payload).then((res) => res.data),

  // 204 No Content (no pending suggestion) comes back as an empty string.
  suggestion: (productId) => apiClient.get(`/products/${productId}/attributes/suggestion`).then((res) => res.data || null),
  generate: (productId) => apiClient.post(`/products/${productId}/attributes/suggestion/generate`).then((res) => res.data),
  confirm: (productId, suggestionId, attributes) => apiClient.post(`/products/${productId}/attributes/suggestion/${suggestionId}/confirm`, { attributes }).then((res) => res.data),
  dismiss: (productId, suggestionId) => apiClient.post(`/products/${productId}/attributes/suggestion/${suggestionId}/dismiss`),

  productTypes: () => apiClient.get('/product-types').then((res) => res.data),
  materials: () => apiClient.get('/materials').then((res) => res.data),
};
