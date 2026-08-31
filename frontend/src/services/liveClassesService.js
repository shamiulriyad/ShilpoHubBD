import apiClient from './apiClient';

export const liveClassesService = {
  list: (params) => apiClient.get('/live-classes', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/live-classes/${id}`).then((res) => res.data),
  registered: () => apiClient.get('/live-classes/registered').then((res) => res.data),
  register: (id) => apiClient.post(`/live-classes/${id}/register`).then((res) => res.data),
  join: (id) => apiClient.post(`/live-classes/${id}/join`),
  leave: (id) => apiClient.post(`/live-classes/${id}/leave`),
  askQuestion: (id, body) => apiClient.post(`/live-classes/${id}/questions`, { body }).then((res) => res.data),
};
