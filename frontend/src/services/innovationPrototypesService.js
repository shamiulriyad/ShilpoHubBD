import apiClient from './apiClient';

const base = '/innovation-lab/prototypes';

export const innovationPrototypesService = {
  list: (params) => apiClient.get(base, { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`${base}/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post(base, payload).then((res) => res.data),
  update: (id, payload) => apiClient.put(`${base}/${id}`, payload).then((res) => res.data),
  remove: (id) => apiClient.delete(`${base}/${id}`).then((res) => res.data),
  addIteration: (id, payload) => apiClient.post(`${base}/${id}/iterations`, payload).then((res) => res.data),
  addTestCase: (id, payload) => apiClient.post(`${base}/${id}/test-cases`, payload).then((res) => res.data),
  removeTestCase: (id, testCaseId) => apiClient.delete(`${base}/${id}/test-cases/${testCaseId}`).then((res) => res.data),
  listIssues: (id) => apiClient.get(`${base}/${id}/issues`).then((res) => res.data),
  addIssue: (id, payload) => apiClient.post(`${base}/${id}/issues`, payload).then((res) => res.data),
  updateIssue: (id, issueId, payload) => apiClient.put(`${base}/${id}/issues/${issueId}`, payload).then((res) => res.data),
};
