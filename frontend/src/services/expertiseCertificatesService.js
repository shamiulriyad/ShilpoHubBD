import apiClient from './apiClient';

export const expertiseCertificatesService = {
  mine: () => apiClient.get('/expertise-certificates/mine').then((res) => res.data),
  eligible: () => apiClient.get('/admin/expertise-certificates/eligible').then((res) => res.data),
  issue: (producerId) => apiClient.post('/admin/expertise-certificates/issue', { producerId }).then((res) => res.data),
};
