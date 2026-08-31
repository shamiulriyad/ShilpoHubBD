import apiClient from './apiClient';

export const aiIntelligenceService = {
  rankSuppliers: (payload) => apiClient.post('/ai-intelligence/supplier-ranking', payload).then((res) => res.data),
  predictQuality: (producerId) => apiClient.post('/ai-intelligence/quality-prediction', { producerId }).then((res) => res.data),
  forecastPrice: (payload) => apiClient.post('/ai-intelligence/price-forecast', payload).then((res) => res.data),
  predictDelivery: (payload) => apiClient.post('/ai-intelligence/delivery-prediction', payload).then((res) => res.data),
  assessRisk: (producerId) => apiClient.post('/ai-intelligence/risk-assessment', { producerId }).then((res) => res.data),
};
