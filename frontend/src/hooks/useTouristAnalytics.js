import { useQuery } from '@tanstack/react-query';
import { touristAnalyticsService } from '../services/touristAnalyticsService';

export function useVisitedLocations() {
  return useQuery({ queryKey: ['tourist-analytics', 'visited-locations'], queryFn: () => touristAnalyticsService.visitedLocations() });
}

export function usePopularDestinations(count = 10) {
  return useQuery({
    queryKey: ['tourist-analytics', 'popular-destinations', count],
    queryFn: () => touristAnalyticsService.popularDestinations(count),
  });
}

export function useTouristBookingStats() {
  return useQuery({ queryKey: ['tourist-analytics', 'bookings'], queryFn: () => touristAnalyticsService.bookings() });
}

export function useFestivalParticipation() {
  return useQuery({
    queryKey: ['tourist-analytics', 'festival-participation'],
    queryFn: () => touristAnalyticsService.festivalParticipation(),
  });
}

export function useDistrictCoverage() {
  return useQuery({ queryKey: ['tourist-analytics', 'district-coverage'], queryFn: () => touristAnalyticsService.districtCoverage() });
}

export function useCulturalAchievements() {
  return useQuery({ queryKey: ['tourist-analytics', 'achievements'], queryFn: () => touristAnalyticsService.achievements() });
}
