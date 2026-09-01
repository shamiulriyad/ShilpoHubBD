import apiClient from './apiClient';

const base = '/innovation-lab/preservation-strategies';

export const preservationStrategiesService = {
  list: (params) => apiClient.get(base, { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`${base}/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post(base, payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`${base}/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`${base}/${id}`).then((res) => res.data),
  addObjective: (id, payload) => apiClient.post(`${base}/${id}/objectives`, payload).then((res) => res.data),
  updateObjective: (id, objectiveId, payload) =>
    apiClient.put(`${base}/${id}/objectives/${objectiveId}`, payload).then((res) => res.data),
  removeObjective: (id, objectiveId) => apiClient.delete(`${base}/${id}/objectives/${objectiveId}`).then((res) => res.data),
  addAction: (id, payload) => apiClient.post(`${base}/${id}/actions`, payload).then((res) => res.data),
  updateAction: (id, actionId, payload) =>
    apiClient.put(`${base}/${id}/actions/${actionId}`, payload).then((res) => res.data),
  removeAction: (id, actionId) => apiClient.delete(`${base}/${id}/actions/${actionId}`).then((res) => res.data),
};
