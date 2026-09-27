import apiClient from './apiClient';

export const producerPartnershipAgreementService = {
  create: (payload) => apiClient.post('/producer-partnership-agreements', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/producer-partnership-agreements', { params }).then((res) => res.data),
  received: (params) => apiClient.get('/producer-partnership-agreements/received', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/producer-partnership-agreements/${id}`).then((res) => res.data),
  updateTerms: (id, payload) => apiClient.put(`/producer-partnership-agreements/${id}/terms`, payload).then((res) => res.data),
  submitForConfirmation: (id) => apiClient.post(`/producer-partnership-agreements/${id}/submit-for-confirmation`).then((res) => res.data),
  confirm: (id) => apiClient.post(`/producer-partnership-agreements/${id}/confirm`).then((res) => res.data),
  suspend: (id, reason) => apiClient.post(`/producer-partnership-agreements/${id}/suspend`, { reason }).then((res) => res.data),
  resume: (id) => apiClient.post(`/producer-partnership-agreements/${id}/resume`).then((res) => res.data),
  complete: (id) => apiClient.post(`/producer-partnership-agreements/${id}/complete`).then((res) => res.data),
  cancel: (id, reason) => apiClient.post(`/producer-partnership-agreements/${id}/cancel`, { reason }).then((res) => res.data),
  getSettlementEligibility: (id) => apiClient.get(`/producer-partnership-agreements/${id}/settlement-eligibility`).then((res) => res.data),
};
