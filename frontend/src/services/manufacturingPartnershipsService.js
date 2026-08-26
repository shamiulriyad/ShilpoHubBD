import apiClient from './apiClient';

export const manufacturingPartnershipsService = {
  create: (payload) => apiClient.post('/manufacturing-partnerships', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/manufacturing-partnerships', { params }).then((res) => res.data),
  received: (params) => apiClient.get('/manufacturing-partnerships/received', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/manufacturing-partnerships/${id}`).then((res) => res.data),
  respond: (id, payload) => apiClient.post(`/manufacturing-partnerships/${id}/respond`, payload).then((res) => res.data),
  addMilestone: (id, payload) => apiClient.post(`/manufacturing-partnerships/${id}/milestones`, payload).then((res) => res.data),
  updateMilestoneStatus: (id, milestoneId, status) =>
    apiClient.post(`/manufacturing-partnerships/${id}/milestones/${milestoneId}/status`, { status }).then((res) => res.data),
  complete: (id) => apiClient.post(`/manufacturing-partnerships/${id}/complete`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/manufacturing-partnerships/${id}/cancel`).then((res) => res.data),
};
