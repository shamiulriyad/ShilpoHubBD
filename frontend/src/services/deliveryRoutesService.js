import apiClient from './apiClient';

export const deliveryRoutesService = {
  list: (params) => apiClient.get('/logistics/routes', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/logistics/routes/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/logistics/routes', payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/logistics/routes/${id}`, payload).then((res) => res.data),
  addStop: (id, payload) => apiClient.post(`/logistics/routes/${id}/stops`, payload).then((res) => res.data),
  removeStop: (id, stopId) => apiClient.delete(`/logistics/routes/${id}/stops/${stopId}`).then((res) => res.data),
  resequence: (id, payload) => apiClient.post(`/logistics/routes/${id}/resequence`, payload).then((res) => res.data),
  optimize: (id, payload) => apiClient.post(`/logistics/routes/${id}/optimize`, payload).then((res) => res.data),
  assign: (id, payload) => apiClient.post(`/logistics/routes/${id}/assign`, payload).then((res) => res.data),
  dispatch: (id) => apiClient.post(`/logistics/routes/${id}/dispatch`, {}).then((res) => res.data),
  start: (id) => apiClient.post(`/logistics/routes/${id}/start`, {}).then((res) => res.data),
  complete: (id) => apiClient.post(`/logistics/routes/${id}/complete`, {}).then((res) => res.data),
  cancel: (id, payload) => apiClient.post(`/logistics/routes/${id}/cancel`, payload).then((res) => res.data),
  arriveStop: (id, stopId) => apiClient.post(`/logistics/routes/${id}/stops/${stopId}/arrive`, {}).then((res) => res.data),
  completeStop: (id, stopId, payload) => apiClient.post(`/logistics/routes/${id}/stops/${stopId}/complete`, payload).then((res) => res.data),
  skipStop: (id, stopId) => apiClient.post(`/logistics/routes/${id}/stops/${stopId}/skip`, {}).then((res) => res.data),
  failStop: (id, stopId, payload) => apiClient.post(`/logistics/routes/${id}/stops/${stopId}/fail`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`/logistics/routes/${id}`).then((res) => res.data),
};
