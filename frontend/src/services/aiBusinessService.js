import apiClient from './apiClient';

export const aiBusinessService = {
  suggestPrice: (payload) => apiClient.post('/producer/ai-business/price-suggestion', payload).then((res) => res.data),
  generateDescription: (payload) => apiClient.post('/producer/ai-business/description', payload).then((res) => res.data),
  translate: (payload) => apiClient.post('/producer/ai-business/translate', payload).then((res) => res.data),
  forecastDemand: (payload) => apiClient.post('/producer/ai-business/demand-forecast', payload).then((res) => res.data),
  planProduction: (payload) => apiClient.post('/producer/ai-business/production-plan', payload).then((res) => res.data),
  forecastMaterials: (payload) => apiClient.post('/producer/ai-business/material-forecast', payload).then((res) => res.data),
  predictSeasonalTrend: (payload) => apiClient.post('/producer/ai-business/seasonal-prediction', payload).then((res) => res.data),
  generateSalesInsights: (payload) => apiClient.post('/producer/ai-business/sales-insights', payload).then((res) => res.data),
};
