import { useQuery } from '@tanstack/react-query';
import { analyticsService } from '../services/analyticsService';

export function usePurchaseAnalytics() {
  return useQuery({ queryKey: ['analytics', 'purchases'], queryFn: () => analyticsService.purchases() });
}

export function useSpendingByMonth(months = 12) {
  return useQuery({
    queryKey: ['analytics', 'spending', months],
    queryFn: () => analyticsService.spendingByMonth(months),
  });
}

export function useFavoriteCategories(count = 5) {
  return useQuery({
    queryKey: ['analytics', 'favorite-categories', count],
    queryFn: () => analyticsService.favoriteCategories(count),
  });
}
