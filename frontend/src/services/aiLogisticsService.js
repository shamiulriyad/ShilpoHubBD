import apiClient from './apiClient';

export const aiLogisticsService = {
  predictDelivery: (payload) => apiClient.post('/logistics/ai/delivery-predictions', payload).then((res) => res.data),
  listDeliveryPredictions: (params) => apiClient.get('/logistics/ai/delivery-predictions', { params }).then((res) => res.data),

  optimizeRoute: (payload) => apiClient.post('/logistics/ai/route-optimizations', payload).then((res) => res.data),
  applyRouteOptimization: (id) => apiClient.post(`/logistics/ai/route-optimizations/${id}/apply`, {}).then((res) => res.data),
  listRouteOptimizations: (params) => apiClient.get('/logistics/ai/route-optimizations', { params }).then((res) => res.data),

  forecastDemand: (payload) => apiClient.post('/logistics/ai/demand-forecasts', payload).then((res) => res.data),
  listDemandForecasts: (params) => apiClient.get('/logistics/ai/demand-forecasts', { params }).then((res) => res.data),

  recommendWarehouse: (payload) => apiClient.post('/logistics/ai/warehouse-allocations', payload).then((res) => res.data),
  listWarehouseAllocations: (params) => apiClient.get('/logistics/ai/warehouse-allocations', { params }).then((res) => res.data),
};
