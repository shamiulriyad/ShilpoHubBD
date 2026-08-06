import apiClient from './apiClient';

export const authService = {
  register: (payload) => apiClient.post('/auth/register', payload).then((res) => res.data),
  login: (payload) => apiClient.post('/auth/login', payload).then((res) => res.data),
  refresh: (refreshToken) => apiClient.post('/auth/refresh', { refreshToken }).then((res) => res.data),
  logout: (refreshToken) => apiClient.post('/auth/logout', { refreshToken }),
  forgotPassword: (email) => apiClient.post('/auth/forgot-password', { email }).then((res) => res.data),
  resetPassword: (payload) => apiClient.post('/auth/reset-password', payload).then((res) => res.data),
  switchRole: (role) => apiClient.post('/auth/switch-role', { role }).then((res) => res.data),
};
