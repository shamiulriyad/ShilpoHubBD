import apiClient from './apiClient';

export const mentorshipRequestsService = {
  create: (payload) => apiClient.post('/mentorship-requests', payload).then((res) => res.data),
  getById: (id) => apiClient.get(`/mentorship-requests/${id}`).then((res) => res.data),
  getMineAsLearner: () => apiClient.get('/mentorship-requests/mine/as-learner').then((res) => res.data),
  getMineAsMentor: () => apiClient.get('/mentorship-requests/mine/as-mentor').then((res) => res.data),
  accept: (id, payload) => apiClient.post(`/mentorship-requests/${id}/accept`, payload).then((res) => res.data),
  reject: (id, payload) => apiClient.post(`/mentorship-requests/${id}/reject`, payload).then((res) => res.data),
  complete: (id) => apiClient.post(`/mentorship-requests/${id}/complete`, {}).then((res) => res.data),
};
