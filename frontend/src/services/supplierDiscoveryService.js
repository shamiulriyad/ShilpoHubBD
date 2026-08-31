import apiClient from './apiClient';

export const supplierDiscoveryService = {
  search: (params) => apiClient.get('/supplier-discovery/search', { params }).then((res) => res.data),
  getProfile: (producerId) => apiClient.get(`/supplier-discovery/producers/${producerId}`).then((res) => res.data),
};
