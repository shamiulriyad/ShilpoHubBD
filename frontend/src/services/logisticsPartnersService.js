import apiClient from './apiClient';

export const logisticsPartnersService = {
  list: (params) => apiClient.get('/logistics/partners', { params }).then((res) => res.data),
  getMine: () => apiClient.get('/logistics/partners/me').then((res) => res.data),
  getByUserId: (userId) => apiClient.get(`/logistics/partners/${userId}`).then((res) => res.data),
  upsert: (userId, payload) => apiClient.put(`/logistics/partners/${userId}`, payload).then((res) => res.data),
  verify: (userId, payload) => apiClient.post(`/logistics/partners/${userId}/verify`, payload).then((res) => res.data),
  upsertServiceArea: (userId, payload) =>
    apiClient.put(`/logistics/partners/${userId}/service-areas`, payload).then((res) => res.data),
  removeServiceArea: (userId, serviceAreaId) =>
    apiClient.delete(`/logistics/partners/${userId}/service-areas/${serviceAreaId}`).then((res) => res.data),
};
