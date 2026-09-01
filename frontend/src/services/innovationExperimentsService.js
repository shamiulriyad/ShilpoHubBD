import apiClient from './apiClient';

const base = '/innovation-lab/experiments';

export const innovationExperimentsService = {
  list: (params) => apiClient.get(base, { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`${base}/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post(base, payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`${base}/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`${base}/${id}`).then((res) => res.data),
  addVersion: (id, payload) => apiClient.post(`${base}/${id}/versions`, payload).then((res) => res.data),
  createRun: (id, payload) => apiClient.post(`${base}/${id}/runs`, payload).then((res) => res.data),
  updateRun: (id, runId, payload) => apiClient.put(`${base}/${id}/runs/${runId}`, payload).then((res) => res.data),
  removeRun: (id, runId) => apiClient.delete(`${base}/${id}/runs/${runId}`).then((res) => res.data),
};
