import apiClient from './apiClient';

export const traceabilityService = {
  getByProduct: (productId) => apiClient.get(`/traceability/products/${productId}`).then((res) => res.data),
};
