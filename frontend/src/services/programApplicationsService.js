import apiClient from './apiClient';

export const programApplicationsService = {
  apply: (payload) => apiClient.post('/program-applications', payload).then((res) => res.data),
  getMine: () => apiClient.get('/program-applications/mine').then((res) => res.data),
  getByProgram: (programId) => apiClient.get(`/program-applications/programs/${programId}`).then((res) => res.data),
  accept: (id, payload) => apiClient.post(`/program-applications/${id}/accept`, payload).then((res) => res.data),
  reject: (id, payload) => apiClient.post(`/program-applications/${id}/reject`, payload).then((res) => res.data),
  withdraw: (id) => apiClient.post(`/program-applications/${id}/withdraw`, {}).then((res) => res.data),
};
