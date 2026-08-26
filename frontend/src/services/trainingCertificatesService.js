import apiClient from './apiClient';

export const trainingCertificatesService = {
  mine: () => apiClient.get('/training-certificates/mine').then((res) => res.data),
  getById: (id) => apiClient.get(`/training-certificates/${id}`).then((res) => res.data),
  verify: (certificateNumber) =>
    apiClient.post('/training-certificates/verify', { certificateNumber }).then((res) => res.data),
};
