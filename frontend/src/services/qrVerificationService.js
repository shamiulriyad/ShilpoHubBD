import apiClient from './apiClient';

export const qrVerificationService = {
  verify: (code) => apiClient.post('/qr-verification/verify', { code }).then((res) => res.data),
  myHistory: (params) => apiClient.get('/qr-verification/history/mine', { params }).then((res) => res.data),
};
