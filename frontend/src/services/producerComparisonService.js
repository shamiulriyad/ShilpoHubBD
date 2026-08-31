import apiClient from './apiClient';

export const producerComparisonService = {
  compare: (producerIds) => apiClient.post('/producer-comparison/compare', { producerIds }).then((res) => res.data),
};
