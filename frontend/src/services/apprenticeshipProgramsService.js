import apiClient from './apiClient';

export const apprenticeshipProgramsService = {
  listPublished: (params) => apiClient.get('/apprenticeship-programs', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/apprenticeship-programs/${id}`).then((res) => res.data),
  getMine: () => apiClient.get('/apprenticeship-programs/mine').then((res) => res.data),
  create: (payload) => apiClient.post('/apprenticeship-programs', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/apprenticeship-programs/${id}`, payload).then((res) => res.data),
  publish: (id) => apiClient.post(`/apprenticeship-programs/${id}/publish`, {}).then((res) => res.data),
  close: (id) => apiClient.post(`/apprenticeship-programs/${id}/close`, {}).then((res) => res.data),
  remove: (id) => apiClient.delete(`/apprenticeship-programs/${id}`).then((res) => res.data),
  addMilestone: (id, payload) => apiClient.post(`/apprenticeship-programs/${id}/milestones`, payload).then((res) => res.data),
  updateMilestone: (id, milestoneId, payload) =>
    apiClient.put(`/apprenticeship-programs/${id}/milestones/${milestoneId}`, payload).then((res) => res.data),
  removeMilestone: (id, milestoneId) =>
    apiClient.delete(`/apprenticeship-programs/${id}/milestones/${milestoneId}`).then((res) => res.data),
};
