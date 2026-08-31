import apiClient from './apiClient';

export const discussionsService = {
  list: (params) => apiClient.get('/discussions', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/discussions/${id}`).then((res) => res.data),
  create: (payload) => apiClient.post('/discussions', payload).then((res) => res.data),
  reply: (id, body) => apiClient.post(`/discussions/${id}/replies`, { body }).then((res) => res.data),
  removeThread: (id) => apiClient.delete(`/discussions/${id}`),
  removeReply: (id, replyId) => apiClient.delete(`/discussions/${id}/replies/${replyId}`),
};
