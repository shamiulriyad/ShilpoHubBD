import apiClient from './apiClient';

export const producerPartnershipSettlementService = {
  generate: (agreementId, periodStart, periodEnd) =>
    apiClient.post(`/producer-partnership-settlements/agreements/${agreementId}/generate`, { periodStart, periodEnd }).then((res) => res.data),
  list: (params) => apiClient.get('/producer-partnership-settlements', { params }).then((res) => res.data),
  getForAgreement: (agreementId) => apiClient.get(`/producer-partnership-settlements/agreements/${agreementId}`).then((res) => res.data),
  getById: (id) => apiClient.get(`/producer-partnership-settlements/${id}`).then((res) => res.data),
  submitForApproval: (id) => apiClient.post(`/producer-partnership-settlements/${id}/submit-for-approval`).then((res) => res.data),
  approve: (id) => apiClient.post(`/producer-partnership-settlements/${id}/approve`).then((res) => res.data),
  reject: (id, reason) => apiClient.post(`/producer-partnership-settlements/${id}/reject`, { reason }).then((res) => res.data),
};
