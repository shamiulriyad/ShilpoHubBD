import apiClient from './apiClient';

export const bookingsService = {
  create: (payload) => apiClient.post('/bookings', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/bookings/mine', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/bookings/${id}`).then((res) => res.data),
  cancel: (id, payload = {}) => apiClient.put(`/bookings/${id}/cancel`, payload).then((res) => res.data),
};
