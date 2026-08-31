import apiClient from './apiClient';

export const csrSponsorshipService = {
  createOpportunity: (payload) => apiClient.post('/csr-sponsorship/opportunities', payload).then((res) => res.data),
  listOpportunities: (params) => apiClient.get('/csr-sponsorship/opportunities', { params }).then((res) => res.data),
  myOpportunities: () => apiClient.get('/csr-sponsorship/opportunities/mine').then((res) => res.data),
  getOpportunity: (id) => apiClient.get(`/csr-sponsorship/opportunities/${id}`).then((res) => res.data),
  closeOpportunity: (id) => apiClient.post(`/csr-sponsorship/opportunities/${id}/close`).then((res) => res.data),
  cancelOpportunity: (id) => apiClient.post(`/csr-sponsorship/opportunities/${id}/cancel`).then((res) => res.data),
  opportunityProposals: (id) => apiClient.get(`/csr-sponsorship/opportunities/${id}/proposals`).then((res) => res.data),
  submitProposal: (id, payload) => apiClient.post(`/csr-sponsorship/opportunities/${id}/proposals`, payload).then((res) => res.data),
  myProposals: (params) => apiClient.get('/csr-sponsorship/proposals', { params }).then((res) => res.data),
  getProposal: (id) => apiClient.get(`/csr-sponsorship/proposals/${id}`).then((res) => res.data),
  decideProposal: (id, payload) => apiClient.post(`/csr-sponsorship/proposals/${id}/decision`, payload).then((res) => res.data),
  addMilestone: (id, payload) => apiClient.post(`/csr-sponsorship/proposals/${id}/milestones`, payload).then((res) => res.data),
  updateMilestoneStatus: (id, milestoneId, status) =>
    apiClient.post(`/csr-sponsorship/proposals/${id}/milestones/${milestoneId}/status`, { status }).then((res) => res.data),
  addProgressUpdate: (id, content) =>
    apiClient.post(`/csr-sponsorship/proposals/${id}/progress-updates`, { content }).then((res) => res.data),
  addImpactRecord: (id, payload) =>
    apiClient.post(`/csr-sponsorship/proposals/${id}/impact-records`, payload).then((res) => res.data),
  completeProposal: (id) => apiClient.post(`/csr-sponsorship/proposals/${id}/complete`).then((res) => res.data),
  cancelProposal: (id) => apiClient.post(`/csr-sponsorship/proposals/${id}/cancel`).then((res) => res.data),
};
