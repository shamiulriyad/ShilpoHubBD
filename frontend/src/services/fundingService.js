import apiClient from './apiClient';

export const fundingService = {
  listPrograms: (params) => apiClient.get('/governance/funding/programs', { params }).then((res) => res.data),
  getProgram: (id) => apiClient.get(`/governance/funding/programs/${id}`).then((res) => res.data),
  createProgram: (payload) => apiClient.post('/governance/funding/programs', payload).then((res) => res.data),
  updateProgram: (id, payload) => apiClient.put(`/governance/funding/programs/${id}`, payload).then((res) => res.data),
  removeProgram: (id) => apiClient.delete(`/governance/funding/programs/${id}`).then((res) => res.data),

  listApplications: (params) => apiClient.get('/governance/funding/applications', { params }).then((res) => res.data),
  getApplication: (id) => apiClient.get(`/governance/funding/applications/${id}`).then((res) => res.data),
  createApplication: (payload) => apiClient.post('/governance/funding/applications', payload).then((res) => res.data),
  submitReview: (id, payload) => apiClient.post(`/governance/funding/applications/${id}/reviews`, payload).then((res) => res.data),
  decideApplication: (id, payload) => apiClient.post(`/governance/funding/applications/${id}/decision`, payload).then((res) => res.data),
  withdrawApplication: (id, payload) => apiClient.post(`/governance/funding/applications/${id}/withdraw`, payload).then((res) => res.data),
  scheduleDisbursement: (id, payload) => apiClient.post(`/governance/funding/applications/${id}/disbursements`, payload).then((res) => res.data),
  updateDisbursementStatus: (id, disbursementId, payload) =>
    apiClient.post(`/governance/funding/applications/${id}/disbursements/${disbursementId}/status`, payload).then((res) => res.data),
  recordRepayment: (id, payload) => apiClient.post(`/governance/funding/applications/${id}/repayments`, payload).then((res) => res.data),
};
