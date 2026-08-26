import apiClient from './apiClient';

export const aiTourismService = {
  tourPlan: (payload) => apiClient.post('/ai-tourism/tour-plan', payload).then((res) => res.data),
  budgetPlan: (payload) => apiClient.post('/ai-tourism/budget-plan', payload).then((res) => res.data),
  routeOptimization: (payload) => apiClient.post('/ai-tourism/route-optimization', payload).then((res) => res.data),
  translate: (payload) => apiClient.post('/ai-tourism/translate', payload).then((res) => res.data),
  culturalRecommendations: (payload) =>
    apiClient.post('/ai-tourism/cultural-recommendations', payload).then((res) => res.data),
};
