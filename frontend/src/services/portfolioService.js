import apiClient from './apiClient';

export const portfolioService = {
  getMine: () => apiClient.get('/portfolios/me').then((res) => res.data),
  getPublic: (academyMemberProfileId) => apiClient.get(`/portfolios/${academyMemberProfileId}`).then((res) => res.data),
  updateMine: (payload) => apiClient.put('/portfolios/me', payload).then((res) => res.data),
  updateVisibility: (payload) => apiClient.put('/portfolios/me/visibility', payload).then((res) => res.data),
  addProject: (payload) => apiClient.post('/portfolios/me/projects', payload).then((res) => res.data),
  updateProject: (projectId, payload) => apiClient.put(`/portfolios/me/projects/${projectId}`, payload).then((res) => res.data),
  removeProject: (projectId) => apiClient.delete(`/portfolios/me/projects/${projectId}`).then((res) => res.data),
};
