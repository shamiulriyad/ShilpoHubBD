import apiClient from './apiClient';

export const businessPartnersService = {
  list: (params) => apiClient.get('/business-partners', { params }).then((res) => res.data),
  getById: (userId) => apiClient.get(`/business-partners/${userId}`).then((res) => res.data),
  upsert: (userId, payload) => apiClient.put(`/business-partners/${userId}`, payload).then((res) => res.data),
  verify: (userId, payload) => apiClient.post(`/business-partners/${userId}/verify`, payload).then((res) => res.data),
};
