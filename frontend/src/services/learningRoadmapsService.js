import apiClient from './apiClient';

export const learningRoadmapsService = {
  create: (payload) => apiClient.post('/learning-roadmaps', payload).then((res) => res.data),
  getActive: () => apiClient.get('/learning-roadmaps/active').then((res) => res.data),
  getById: (id) => apiClient.get(`/learning-roadmaps/${id}`).then((res) => res.data),
  getHistory: () => apiClient.get('/learning-roadmaps/history').then((res) => res.data),
  refreshProgress: (id) => apiClient.post(`/learning-roadmaps/${id}/refresh`, {}).then((res) => res.data),
  completeMilestone: (id, milestoneId) =>
    apiClient.post(`/learning-roadmaps/${id}/milestones/${milestoneId}/complete`, {}).then((res) => res.data),
};
