import apiClient from './apiClient';

export const logisticsPartnersService = {
  createAccount: (profileId, payload) => apiClient.post(`/admin/users/logistics-partners/${profileId}/account`, payload).then((res) => res.data),
  available: (districtId, areaName) => apiClient.get('/logistics/partners/available', {
    params: { districtId, areaName: areaName || undefined },
  }).then((res) => res.data),
  createOfficial: (payload) => apiClient.post('/logistics/partners/official', payload).then((res) => res.data),
  getOfficial: (profileId) => apiClient.get(`/logistics/partners/official/${profileId}`).then((res) => res.data),
  performance: (profileId) => apiClient.get(`/logistics/partners/official/${profileId}/performance`).then((res) => res.data),
  updateOfficial: (profileId, payload) => apiClient.put(`/logistics/partners/official/${profileId}`, payload).then((res) => res.data),
  upsertOfficialServiceArea: (profileId, payload) => apiClient.put(`/logistics/partners/official/${profileId}/service-areas`, payload).then((res) => res.data),
  removeOfficialServiceArea: (profileId, serviceAreaId) => apiClient.delete(`/logistics/partners/official/${profileId}/service-areas/${serviceAreaId}`).then((res) => res.data),
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
