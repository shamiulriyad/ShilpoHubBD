import apiClient from './apiClient';

export const investmentOpportunitiesService = {
  create: (payload) => apiClient.post('/investment-opportunities', payload).then((res) => res.data),
  list: (params) => apiClient.get('/investment-opportunities', { params }).then((res) => res.data),
  mine: () => apiClient.get('/investment-opportunities/mine').then((res) => res.data),
  getById: (id) => apiClient.get(`/investment-opportunities/${id}`).then((res) => res.data),
  close: (id) => apiClient.post(`/investment-opportunities/${id}/close`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/investment-opportunities/${id}/cancel`).then((res) => res.data),
  opportunityProposals: (id) => apiClient.get(`/investment-opportunities/${id}/proposals`).then((res) => res.data),
  submitProposal: (id, payload) => apiClient.post(`/investment-opportunities/${id}/proposals`, payload).then((res) => res.data),
  myProposals: () => apiClient.get('/investment-opportunities/proposals/mine').then((res) => res.data),
  getProposal: (id) => apiClient.get(`/investment-opportunities/proposals/${id}`).then((res) => res.data),
  decideProposal: (id, payload) => apiClient.post(`/investment-opportunities/proposals/${id}/decision`, payload).then((res) => res.data),
  addMilestone: (id, payload) => apiClient.post(`/investment-opportunities/proposals/${id}/milestones`, payload).then((res) => res.data),
  updateMilestoneStatus: (id, milestoneId, status) =>
    apiClient.post(`/investment-opportunities/proposals/${id}/milestones/${milestoneId}/status`, { status }).then((res) => res.data),
  addDocument: (id, payload) => apiClient.post(`/investment-opportunities/proposals/${id}/documents`, payload).then((res) => res.data),
  completeProposal: (id) => apiClient.post(`/investment-opportunities/proposals/${id}/complete`).then((res) => res.data),
  cancelProposal: (id) => apiClient.post(`/investment-opportunities/proposals/${id}/cancel`).then((res) => res.data),
};
