import apiClient from './apiClient';

export const achievementsService = {
  myXpSummary: () => apiClient.get('/achievements/xp/mine').then((res) => res.data),
  myXpHistory: () => apiClient.get('/achievements/xp/mine/history').then((res) => res.data),
  listAll: () => apiClient.get('/achievements').then((res) => res.data),
  mine: () => apiClient.get('/achievements/mine').then((res) => res.data),
  evaluate: () => apiClient.post('/achievements/evaluate').then((res) => res.data),
};
