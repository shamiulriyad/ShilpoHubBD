import apiClient from './apiClient';

export const analyticsService = {
  purchases: () => apiClient.get('/analytics/purchases').then((res) => res.data),
  spendingByMonth: (months = 12) => apiClient.get('/analytics/spending', { params: { months } }).then((res) => res.data),
  favoriteCategories: (count = 5) =>
    apiClient.get('/analytics/favorite-categories', { params: { count } }).then((res) => res.data),
};
