import apiClient from './apiClient';

export const heritageIdentityService = {
  getByProducer: (producerId) => apiClient.get(`/heritage-identity/${producerId}`).then((res) => res.data),
  getScore: (producerId) => apiClient.get(`/heritage-identity/${producerId}/score`).then((res) => res.data),
  getVerified: (params) => apiClient.get('/heritage-identity/verified', { params }).then((res) => res.data),
  verify: (producerId, payload) => apiClient.post(`/heritage-identity/${producerId}/verify`, payload).then((res) => res.data),
};
