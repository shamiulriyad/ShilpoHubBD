import { useQuery } from '@tanstack/react-query';
import { businessPartnerAnalyticsService } from '../services/businessPartnerAnalyticsService';

export function useMarketDemand(params = {}) {
  return useQuery({ queryKey: ['bp-analytics', 'market-demand', params], queryFn: () => businessPartnerAnalyticsService.marketDemand(params) });
}

export function useSpendingAnalytics() {
  return useQuery({ queryKey: ['bp-analytics', 'spending'], queryFn: () => businessPartnerAnalyticsService.spending() });
}

export function useProcurementAnalytics() {
  return useQuery({ queryKey: ['bp-analytics', 'procurement'], queryFn: () => businessPartnerAnalyticsService.procurement() });
}

export function useSupplierPerformance() {
  return useQuery({ queryKey: ['bp-analytics', 'supplier-performance'], queryFn: () => businessPartnerAnalyticsService.supplierPerformance() });
}

export function useOrderTrends() {
  return useQuery({ queryKey: ['bp-analytics', 'order-trends'], queryFn: () => businessPartnerAnalyticsService.orderTrends() });
}

export function useIndustryInsights(params = {}) {
  return useQuery({ queryKey: ['bp-analytics', 'industry-insights', params], queryFn: () => businessPartnerAnalyticsService.industryInsights(params) });
}
