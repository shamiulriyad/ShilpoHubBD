import apiClient from './apiClient';

export const identityVerificationService = {
  submit: (payload) => apiClient.post('/identity-verifications', payload).then((res) => res.data),
  mine: () => apiClient.get('/identity-verifications/me').then((res) => res.data),

  list: (params) => apiClient.get('/identity-verifications', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/identity-verifications/${id}`).then((res) => res.data),
  approve: (id) => apiClient.post(`/identity-verifications/${id}/approve`).then((res) => res.data),
  reject: (id, rejectionReason) =>
    apiClient.post(`/identity-verifications/${id}/reject`, { rejectionReason }).then((res) => res.data),
};
