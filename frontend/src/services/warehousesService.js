import apiClient from './apiClient';

export const warehousesService = {
  list: (params) => apiClient.get('/logistics/warehouses', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/logistics/warehouses/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/logistics/warehouses', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/logistics/warehouses/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/logistics/warehouses/${id}`).then((res) => res.data),
  addZone: (id, payload) => apiClient.post(`/logistics/warehouses/${id}/zones`, payload).then((res) => res.data),
  updateZone: (id, zoneId, payload) =>
    apiClient.put(`/logistics/warehouses/${id}/zones/${zoneId}`, payload).then((res) => res.data),
  removeZone: (id, zoneId) =>
    apiClient.delete(`/logistics/warehouses/${id}/zones/${zoneId}`).then((res) => res.data),
  addBin: (id, payload) => apiClient.post(`/logistics/warehouses/${id}/bins`, payload).then((res) => res.data),
  updateBin: (id, binId, payload) =>
    apiClient.put(`/logistics/warehouses/${id}/bins/${binId}`, payload).then((res) => res.data),
  removeBin: (id, binId) =>
    apiClient.delete(`/logistics/warehouses/${id}/bins/${binId}`).then((res) => res.data),
};
