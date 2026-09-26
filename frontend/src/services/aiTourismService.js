import apiClient from './apiClient';

export const aiTourismService = {
  tourPlan: (payload) => apiClient.post('/ai-tourism/tour-plan', payload, { timeout: 90000 }).then((res) => res.data),
  savedPlans: (params) => apiClient.get('/ai-tourism/saved-plans', { params }).then((res) => res.data),
  savedPlan: (id) => apiClient.get(`/ai-tourism/saved-plans/${id}`).then((res) => res.data),
  deleteSavedPlan: (id) => apiClient.delete(`/ai-tourism/saved-plans/${id}`),
  budgetPlan: (payload) => apiClient.post('/ai-tourism/budget-plan', payload).then((res) => res.data),
  routeOptimization: (payload) => apiClient.post('/ai-tourism/route-optimization', payload).then((res) => res.data),
  translate: (payload) => apiClient.post('/ai-tourism/translate', payload).then((res) => res.data),
  culturalRecommendations: (payload) =>
    apiClient.post('/ai-tourism/cultural-recommendations', payload).then((res) => res.data),
};
