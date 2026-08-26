import apiClient from './apiClient';

export const touristAnalyticsService = {
  visitedLocations: () => apiClient.get('/tourist-analytics/visited-locations').then((res) => res.data),
  popularDestinations: (count = 10) =>
    apiClient.get('/tourist-analytics/popular-destinations', { params: { count } }).then((res) => res.data),
  bookings: () => apiClient.get('/tourist-analytics/bookings').then((res) => res.data),
  spending: (months = 12) => apiClient.get('/tourist-analytics/spending', { params: { months } }).then((res) => res.data),
  favoriteCategories: (count = 5) =>
    apiClient.get('/tourist-analytics/favorite-categories', { params: { count } }).then((res) => res.data),
  festivalParticipation: () => apiClient.get('/tourist-analytics/festival-participation').then((res) => res.data),
  districtCoverage: () => apiClient.get('/tourist-analytics/district-coverage').then((res) => res.data),
  achievements: () => apiClient.get('/tourist-analytics/achievements').then((res) => res.data),
};
