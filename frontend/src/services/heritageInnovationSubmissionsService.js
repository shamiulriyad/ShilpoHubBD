import apiClient from './apiClient';

const base = '/innovation-lab/submissions';

export const heritageInnovationSubmissionsService = {
  list: (params) => apiClient.get(base, { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`${base}/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post(base, payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`${base}/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`${base}/${id}`).then((res) => res.data),
  submit: (id) => apiClient.post(`${base}/${id}/submit`, {}).then((res) => res.data),
  withdraw: (id) => apiClient.post(`${base}/${id}/withdraw`, {}).then((res) => res.data),
  addTeamMember: (id, payload) => apiClient.post(`${base}/${id}/team-members`, payload).then((res) => res.data),
  removeTeamMember: (id, memberId) => apiClient.delete(`${base}/${id}/team-members/${memberId}`).then((res) => res.data),
  addReview: (id, payload) => apiClient.post(`${base}/${id}/reviews`, payload).then((res) => res.data),
  getHistory: (id, take = 50) => apiClient.get(`${base}/${id}/history`, { params: { take } }).then((res) => res.data),
};
