import apiClient from './apiClient';

export const sustainabilityService = {
  me: () => apiClient.get('/sustainability/me').then((res) => res.data),
  getByProducer: (producerId) => apiClient.get(`/sustainability/producers/${producerId}`).then((res) => res.data),
  addMaterial: (payload) => apiClient.post('/sustainability/materials', payload).then((res) => res.data),
  addCertification: (payload) => apiClient.post('/sustainability/certifications', payload).then((res) => res.data),
  verifyCertification: (certificationId) =>
    apiClient.post(`/sustainability/certifications/${certificationId}/verify`).then((res) => res.data),
};
