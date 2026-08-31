import apiClient from './apiClient';

export const supplierMatchingService = {
  match: (payload) => apiClient.post('/supplier-matching/match', payload).then((res) => res.data),
};
