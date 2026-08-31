import apiClient from './apiClient';

export const businessPartnerAnalyticsService = {
  marketDemand: (params) => apiClient.get('/business-partner-analytics/market-demand', { params }).then((res) => res.data),
  exportTrends: (params) => apiClient.get('/business-partner-analytics/export-trends', { params }).then((res) => res.data),
  productionForecast: (categoryId, horizonMonths = 3) =>
    apiClient.get('/business-partner-analytics/production-forecast', { params: { categoryId, horizonMonths } }).then((res) => res.data),
  industryInsights: (params) => apiClient.get('/business-partner-analytics/industry-insights', { params }).then((res) => res.data),
  procurement: () => apiClient.get('/business-partner-analytics/procurement').then((res) => res.data),
  supplierPerformance: () => apiClient.get('/business-partner-analytics/supplier-performance').then((res) => res.data),
  spending: () => apiClient.get('/business-partner-analytics/spending').then((res) => res.data),
  orderTrends: () => apiClient.get('/business-partner-analytics/order-trends').then((res) => res.data),
};
